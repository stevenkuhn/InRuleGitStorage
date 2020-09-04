// Install modules
#module nuget:?package=Cake.DotNetTool.Module&version=0.4.0

// Install addins.
#addin nuget:?package=Cake.Incubator&version=5.1.0
#addin nuget:?package=Cake.OctoDeploy&version=3.2.0

// Install tools.
#tool dotnet:?package=GitVersion.Tool&version=5.1.3
#tool nuget:?package=NuGet.CommandLine&version=5.4.0

// Load other scripts.
#load "./build/parameters.cake"

//////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
//////////////////////////////////////////////////////////////////////

Setup<BuildParameters>(context =>
{
  var buildParameters = new BuildParameters(context);

  return buildParameters;
});

Teardown<BuildParameters>((context, parameters) =>
{

});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
  .WithCriteria<BuildParameters>((context, build) => !build.IsSkippingBuild)
  .Does<BuildParameters>(build => 
{
  DeleteDirectories(GetDirectories("./**/obj"), new DeleteDirectorySettings { Force = true, Recursive = true });
  DeleteDirectories(GetDirectories("./**/bin"), new DeleteDirectorySettings { Force = true, Recursive = true });

  /*DotNetCoreClean(build.Files.SdkProject, new DotNetCoreCleanSettings 
  {
    Configuration = build.Configuration 
  });
    
  DotNetCoreClean(build.Files.SdkTestProject, new DotNetCoreCleanSettings
  { 
    Configuration = build.Configuration
  });

  if (build.IsRunningOnWindows)
  {
    MSBuild(build.Files.AuthoringProject, new MSBuildSettings 
    { 
      Configuration = build.Configuration,
      Targets = { "Clean" },
      ToolVersion = MSBuildToolVersion.VS2019
    });
  }*/
});

Task("Clean-Artifacts")
  .WithCriteria<BuildParameters>((context, build) => !build.IsSkippingBuild)
  .Does<BuildParameters>(build => 
{
  if (DirectoryExists(build.Directories.Artifacts))
  {
    DeleteDirectory(build.Directories.Artifacts, new DeleteDirectorySettings { Force = true, Recursive = true });
  }
});

Task("Clean-TestResults")
  .WithCriteria<BuildParameters>((context, build) => !build.IsSkippingBuild)
  .Does<BuildParameters>(build => 
{
  DeleteDirectories(
    build.Directories.TestResults, 
    new DeleteDirectorySettings { Force = true, Recursive = true });
});

Task("Restore")
  .WithCriteria<BuildParameters>((context, build) => !build.IsSkippingBuild)
  .Does<BuildParameters>(build => 
{
  Information("Restoring SDK project NuGet packages...");
  DotNetCoreRestore(build.Files.SdkProject, new DotNetCoreRestoreSettings()
  {
    Sources = new [] { 
      "https://api.nuget.org/v3/index.json",
      "https://www.myget.org/F/stevenkuhn/api/v3/index.json"
    },
  });

  Information("Restoring SDK test project NuGet packages...");
  DotNetCoreRestore(build.Files.SdkTestProject, new DotNetCoreRestoreSettings()
  {
    Sources = new [] { 
      "https://api.nuget.org/v3/index.json",
      "https://www.myget.org/F/stevenkuhn/api/v3/index.json"
    },
  });

  Information($"Updating NuGet package InRule.Repository v{build.InRule.Version} for SDK project.");
  DotNetCoreTool(build.Files.SdkProject, "add", $"package InRule.Repository --no-restore --version {build.InRule.Version}");

  if (build.IsRunningOnWindows)
  {
    Information("Restoring Authoring project NuGet packages...");
    NuGetRestore(build.Files.AuthoringProject, new NuGetRestoreSettings
    { 
      PackagesDirectory = "./packages",
      Source = new [] { 
        "https://api.nuget.org/v3/index.json",
        "https://www.myget.org/F/stevenkuhn/api/v3/index.json"
      },
    });

    Information($"Update NuGet package InRule.Authoring.SDK v{build.InRule.Version} for Authoring project.");
    NuGetUpdate(build.Files.AuthoringProject, new NuGetUpdateSettings
    {
      ArgumentCustomization = args => args.Append("-RepositoryPath ./packages"),
      Id = new [] { "InRule.Authoring.SDK" },
      Source = new []
      {
        "https://api.nuget.org/v3/index.json",
        "https://www.myget.org/F/stevenkuhn/api/v3/index.json"
      },
      Version = build.InRule.Version
    });
  }
});

Task("Build")
  .IsDependentOn("Restore")
  .WithCriteria<BuildParameters>((context, build) => !build.IsSkippingBuild)
  .Does<BuildParameters>(build =>
{
  DotNetCoreBuild(build.Files.SdkProject, new DotNetCoreBuildSettings
  {
    ArgumentCustomization = args => args.Append($"/p:Version={build.Version.FullSemanticVersion}")
                                        .Append($"/p:AssemblyVersion={build.Version.AssemblySemanticVersion}")
                                        .Append($"/p:InformationalVersion={build.Version.InformationalVersion}"),
    Configuration = build.Configuration,
    NoRestore = true,
    Framework = build.IsRunningOnWindows ? null : "netstandard2.0"
  });

  DotNetCoreBuild(build.Files.SdkTestProject, new DotNetCoreBuildSettings
  {
    ArgumentCustomization = args => args.Append($"/p:Version={build.Version.FullSemanticVersion}")
                                        .Append($"/p:AssemblyVersion={build.Version.AssemblySemanticVersion}")
                                        .Append($"/p:InformationalVersion={build.Version.InformationalVersion}"),
    Configuration = build.Configuration,
    NoRestore = true,
    Framework = build.IsRunningOnWindows ? null : "netcoreapp3.1"
  });

  if (build.IsRunningOnWindows)
  {
    MSBuild(build.Files.AuthoringProject, new MSBuildSettings
    {
      ArgumentCustomization = args => args.Append($"/p:Version={build.Version.FullSemanticVersion}")
                                          .Append($"/p:AssemblyVersion={build.Version.AssemblySemanticVersion}")
                                          .Append($"/p:InformationalVersion={build.Version.InformationalVersion}"),
      Configuration = build.Configuration,
      Restore = false,
      ToolVersion = MSBuildToolVersion.VS2019,
    });
  }
});

Task("Test")
  .IsDependentOn("Clean-TestResults")
  .IsDependentOn("Build")
  .WithCriteria<BuildParameters>((context, build) => !build.IsSkippingBuild)
  .Does<BuildParameters>(build =>
{
  DotNetCoreTest(build.Files.SdkTestProject, new DotNetCoreTestSettings
  {
    Configuration = build.Configuration,
    Framework = "netcoreapp3.1",
    NoBuild = true,
    NoRestore = true,
    Logger = "trx;LogFileName=./netcoreapp3.1/TestResult.trx",
  });

  if (build.IsRunningOnWindows)
  {
    DotNetCoreTest(build.Files.SdkTestProject, new DotNetCoreTestSettings
    {
      Configuration = build.Configuration,
      Framework = "net461",
      NoBuild = true,
      NoRestore = true,
      Logger = "trx;LogFileName=./net461/TestResult.trx"
    });
  }
});

Task("Publish-Artifacts")
  .IsDependentOn("Build")
  .IsDependentOn("Clean-Artifacts")
  .WithCriteria<BuildParameters>((context, build) => !build.IsSkippingBuild)
  .Does<BuildParameters>(build =>
{
  if (!DirectoryExists(build.Directories.Artifacts)) { CreateDirectory(build.Directories.Artifacts); }

  CopyFiles($"./src/**/{build.Configuration}/**/Sknet.InRuleGitStorage.{build.Version.SemanticVersion}.nupkg", build.Directories.Artifacts);
  CopyFiles($"./src/**/{build.Configuration}/**/Sknet.InRuleGitStorage.{build.Version.SemanticVersion}.snupkg", build.Directories.Artifacts);

  if (build.IsRunningOnWindows)
  {
    if (!DirectoryExists($"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.AuthoringExtension")) { CreateDirectory($"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.AuthoringExtension"); }
    if (!DirectoryExists($"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.AuthoringExtension/lib")) { CreateDirectory($"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.AuthoringExtension/lib"); }
    if (!DirectoryExists($"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.AuthoringExtension/lib/win32")) { CreateDirectory($"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.AuthoringExtension/lib/win32"); }

    CopyFiles($"./src/Sknet.InRuleGitStorage.AuthoringExtension/bin/{build.Configuration}/Sknet.InRuleGitStorage.AuthoringExtension.*", $"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.AuthoringExtension/");
    CopyFiles($"./src/Sknet.InRuleGitStorage.AuthoringExtension/bin/{build.Configuration}/Sknet.InRuleGitStorage.*", $"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.AuthoringExtension/");
    
    CopyFiles($"./src/Sknet.InRuleGitStorage.AuthoringExtension/bin/{build.Configuration}/LibGit2Sharp.*", $"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.AuthoringExtension/", true);
    CopyFiles($"./src/Sknet.InRuleGitStorage.AuthoringExtension/bin/{build.Configuration}/lib/win32/**/*", $"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.AuthoringExtension/lib/win32/", true);
    DeleteFiles($"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.AuthoringExtension/**/*.xml");
    
    Zip($"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.AuthoringExtension/", $"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.AuthoringExtension.{build.Version.SemanticVersion}.zip");
    
    DeleteDirectory($"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.AuthoringExtension/", new DeleteDirectorySettings
    {
      Force = true,
      Recursive = true,
    });
  }
});

Task("Deploy-To-irAuthor")
  .IsDependentOn("Publish-Artifacts")
  .WithCriteria<BuildParameters>((context, build) => !build.IsSkippingBuild)
  .Does<BuildParameters>(build =>
  {
    if (DirectoryExists(build.Directories.IrAuthorExtensions)) { DeleteDirectory(build.Directories.IrAuthorExtensions, new DeleteDirectorySettings { Force = true, Recursive = true }); }
    CreateDirectory(build.Directories.IrAuthorExtensions);

    Unzip($"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.AuthoringExtension.{build.Version.SemanticVersion}.zip", build.Directories.IrAuthorExtensions);
  });

Task("Publish-To-GitHub")
  .IsDependentOn("Publish-Artifacts")
  .Does<BuildParameters>(build => 
{
  if (string.IsNullOrWhiteSpace(build.GitHub.AccessToken))
  {
    throw new InvalidOperationException("Cannot create release in GitHub. You must provide a GitHub access token via the 'githubAccessToken' command-line argument or the 'GitHub_Access_Token' environment variable.");
  }

  var artifacts = new List<FilePath>();
  artifacts.Add($"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.{build.Version.SemanticVersion}.nupkg");
  artifacts.Add($"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.{build.Version.SemanticVersion}.snupkg");
  artifacts.Add($"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.AuthoringExtension.{build.Version.SemanticVersion}.zip");
  artifacts = artifacts.OrderBy(x => x.GetFilename().ToString()).ToList();

  PublishReleaseWithArtifacts(
    tag: $"v{build.Version.SemanticVersion}",
    releaseTitle: $"v{build.Version.SemanticVersion}",
    releaseNotes: $"Release notes for `v{build.Version.SemanticVersion}` are not available at this time.",
    draftRelease: false,
    preRelease: !string.IsNullOrWhiteSpace(build.Version.PreReleaseTag),
    artifactPaths: artifacts.ToArray(),
    artifactNames: artifacts.Select(x => x.GetFilename().ToString()).ToArray(),
    artifactMimeTypes: artifacts.Select(x => "application/zip").ToArray(),
    octoDeploySettings: new OctoDeploySettings
    {
      AccessToken = build.GitHub.AccessToken,
      Owner = "stevenkuhn",
      Repository = "InRuleGitStorage"
    }
  );
});

Task("Publish-To-NuGet-Feed")
  .IsDependentOn("Publish-Artifacts")
  .Does<BuildParameters>(build => 
{
  if (string.IsNullOrWhiteSpace(build.NuGet.ApiKey))
  {
    throw new InvalidOperationException("Cannot publish NuGet package(s) to the NuGet feed. You must provide a NuGet API key via the 'nugetApiKey' command-line argument or the 'NuGet_ApiKey' environment variable.");
  }

  NuGetPush($"{build.Directories.Artifacts}/Sknet.InRuleGitStorage.{build.Version.SemanticVersion}.nupkg", new NuGetPushSettings {
    ApiKey = build.NuGet.ApiKey,
    Source = "https://api.nuget.org/v3/index.json"
  });
});

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
  .IsDependentOn("Test")
  .IsDependentOn("Deploy-To-irAuthor");

Task("CI")
  .IsDependentOn("Test")
  .IsDependentOn("Publish-Artifacts");

Task("Release")
  .IsDependentOn("Clean")
  .IsDependentOn("Test")
  .IsDependentOn("Publish-To-GitHub")
  .IsDependentOn("Publish-To-NuGet-Feed");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(Argument("target", "Default"));