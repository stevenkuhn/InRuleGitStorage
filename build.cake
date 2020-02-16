// Install addins.
#addin nuget:?package=Cake.Incubator&version=5.1.0
#addin nuget:?package=Cake.OctoDeploy&version=3.2.0

// Install tools.
#tool nuget:?package=GitVersion.CommandLine&version=5.1.3
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

/*Teardown<BuildParameters>((context, parameters) =>
{

});*/

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
  .Does<BuildParameters>(build => 
{
  DotNetCoreClean(build.Files.SdkProject, new DotNetCoreCleanSettings 
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
  }
});

/*Task("Clean-Artifacts")
  .Does(() => 
  {
    if (DirectoryExists(artifactsFolder))
    {
      DeleteDirectory(artifactsFolder, new DeleteDirectorySettings { Force = true, Recursive = true });
    }
  });*/

Task("Clean-TestResults")
  .Does<BuildParameters>(build => 
{
  DeleteDirectories(
    build.Directories.TestResults, 
    new DeleteDirectorySettings { Force = true, Recursive = true });
});

Task("Restore")
  .Does<BuildParameters>(build => 
{
  Information($"Updating NuGet package InRule.Repository v{build.InRule.Version} for SDK project.");
  DotNetCoreTool(build.Files.SdkProject, "add", $"package InRule.Repository --version {build.InRule.Version}");

  Information("Restoring SDK project NuGet packages...");
  DotNetCoreRestore(build.Files.SdkProject);

  Information("Restoring SDK test project NuGet packages...");
  DotNetCoreRestore(build.Files.SdkTestProject);

  if (build.IsRunningOnWindows)
  {
    Information("Restoring Authoring project NuGet packages...");
    NuGetRestore(build.Files.AuthoringProject, new NuGetRestoreSettings
    { 
      PackagesDirectory = build.Directories.Packages,
      Source = new [] { "https://api.nuget.org/v3/index.json" },
    });

    Information($"Update NuGet package InRule.Authoring.SDK v{build.InRule.Version} for Authoring project.");
    NuGetUpdate(build.Files.AuthoringProject, new NuGetUpdateSettings
    {
      Id = new [] { "InRule.Authoring.SDK" },
      Source = new [] { "https://api.nuget.org/v3/index.json" },
      Version = build.InRule.Version, 
    });
  }
});

Task("Build")
  .IsDependentOn("Restore")
  .Does<BuildParameters>(build =>
{
  DotNetCoreBuild(build.Files.SdkProject, new DotNetCoreBuildSettings
  {
    //ArgumentCustomization = args => args.Append($"/p:Version={fullSemVer}")
    //                                    .Append($"/p:AssemblyVersion={assemblySemVer}")
    //                                    .Append($"/p:InformationalVersion={informationalVersion}"),
    Configuration = build.Configuration,
    NoRestore = true,
    Framework = build.IsRunningOnWindows ? null : "netstandard2.0"
  });

  DotNetCoreBuild(build.Files.SdkTestProject, new DotNetCoreBuildSettings
  {
    //ArgumentCustomization = args => args.Append($"/p:Version={fullSemVer}")
    //                                    .Append($"/p:AssemblyVersion={assemblySemVer}")
    //                                    .Append($"/p:InformationalVersion={informationalVersion}"),
    Configuration = build.Configuration,
    NoRestore = true,
    Framework = build.IsRunningOnWindows ? null : "netcoreapp3.1"
  });

  if (build.IsRunningOnWindows)
  {
    MSBuild(build.Files.AuthoringProject, new MSBuildSettings
    {
      //ArgumentCustomization = args => args.Append($"/p:Version={fullSemVer}")
      //                                    .Append($"/p:AssemblyVersion={assemblySemVer}")
      //                                    .Append($"/p:InformationalVersion={informationalVersion}"),
      Configuration = build.Configuration,
      Restore = false,
      ToolVersion = MSBuildToolVersion.VS2019,
    });
  }
});

Task("Test")
  .IsDependentOn("Clean-TestResults")
  .IsDependentOn("Build")
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

/*
Task("Publish-To-Folder")
  .IsDependentOn("Clean-Artifacts")
  .IsDependentOn("Build-Sdk")
  .IsDependentOn("Build-AuthoringExtension")
  .Does(() =>
  {
    if (!DirectoryExists(artifactsFolder)) { CreateDirectory(artifactsFolder); }
    
    CopyFiles($"./src/**a/{configuration}/**a/Sknet.InRuleGitStorage.{gitVersion.SemVer}.nupkg", artifactsFolder);
    CopyFiles($"./src/**a/{configuration}/**a/Sknet.InRuleGitStorage.{gitVersion.SemVer}.snupkg", artifactsFolder);

    if (!DirectoryExists($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension")) { CreateDirectory($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension"); }
    if (!DirectoryExists($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/lib")) { CreateDirectory($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/lib"); }
    if (!DirectoryExists($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/lib/win32")) { CreateDirectory($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/lib/win32"); }

    CopyFiles($"./src/Sknet.InRuleGitStorage.AuthoringExtension/bin/{configuration}/Sknet.InRuleGitStorage.AuthoringExtension.*", $"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/");
    CopyFiles($"./src/Sknet.InRuleGitStorage.AuthoringExtension/bin/{configuration}/Sknet.InRuleGitStorage.*", $"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/");
    
    CopyFiles($"./src/Sknet.InRuleGitStorage.AuthoringExtension/bin/{configuration}/LibGit2Sharp.*", $"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/", true);
    CopyFiles($"./src/Sknet.InRuleGitStorage.AuthoringExtension/bin/{configuration}/lib/win32/**a/*", $"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/lib/win32/", true);
    DeleteFiles($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/**a/*.xml");
    
    Zip($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/", $"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension.{gitVersion.SemVer}.zip");
    
    DeleteDirectory($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/", new DeleteDirectorySettings
    {
      Force = true,
      Recursive = true,
    });
  });

Task("Deploy-To-irAuthor")
  .IsDependentOn("Publish-To-Folder")
  .Does(() =>
  {
    var extensionFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/InRule/irAuthor/ExtensionExchange/Sknet.InRuleGitStorage";

    if (DirectoryExists(extensionFolder)) { DeleteDirectory(extensionFolder, new DeleteDirectorySettings { Force = true, Recursive = true }); }
    CreateDirectory(extensionFolder);

    Unzip($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension.{gitVersion.SemVer}.zip", extensionFolder);
  });

Task("Publish-To-GitHub")
  .IsDependentOn("Publish-To-Folder")
  .Does(() => 
  {
    if (string.IsNullOrWhiteSpace(githubAccessToken))
    {
      throw new InvalidOperationException("Cannot create release in GitHub. You must provide a GitHub access token via the 'githubAccessToken' command-line argument or the 'GitHub_Access_Token' environment variable.");
    }

    var artifacts = new List<FilePath>();
    artifacts.Add($"{artifactsFolder}/Sknet.InRuleGitStorage.{gitVersion.SemVer}.nupkg");
    artifacts.Add($"{artifactsFolder}/Sknet.InRuleGitStorage.{gitVersion.SemVer}.snupkg");
    artifacts.Add($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension.{gitVersion.SemVer}.zip");
    artifacts = artifacts.OrderBy(x => x.GetFilename().ToString()).ToList();

    PublishReleaseWithArtifacts(
      tag: $"v{gitVersion.SemVer}",
      releaseTitle: $"v{gitVersion.SemVer}",
      releaseNotes: $"Release notes for `v{gitVersion.SemVer}` are not available at this time.",
      draftRelease: false,
      preRelease: !string.IsNullOrWhiteSpace(gitVersion.PreReleaseTag),
      artifactPaths: artifacts.ToArray(),
      artifactNames: artifacts.Select(x => x.GetFilename().ToString()).ToArray(),
      artifactMimeTypes: artifacts.Select(x => "application/zip").ToArray(),
      octoDeploySettings: new OctoDeploySettings
      {
        AccessToken = githubAccessToken,
        Owner = "stevenkuhn",
        Repository = "InRuleGitStorage"
      }
    );
  });

Task("Publish-To-NuGet-Feed")
  .IsDependentOn("Publish-To-Folder")
  .Does(() =>
  {
    if (string.IsNullOrWhiteSpace(nugetApiKey))
    {
      throw new InvalidOperationException("Cannot publish NuGet package(s) to the NuGet feed. You must provide a NuGet API key via the 'nugetApiKey' command-line argument or the 'NuGet_ApiKey' environment variable.");
    }

    NuGetPush($"{artifactsFolder}/Sknet.InRuleGitStorage.{gitVersion.SemVer}.nupkg", new NuGetPushSettings {
      ApiKey = nugetApiKey,
      Source = "https://api.nuget.org/v3/index.json"
    });
  });*/

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

/*Task("Clean")
  .IsDependentOn("Clean-Sdk")
  .IsDependentOn("Clean-AuthoringExtension")
  .IsDependentOn("Clean-Artifacts")
  .IsDependentOn("Clean-TestResults");

Task("Restore")
  .IsDependentOn("Restore-Sdk")
  .IsDependentOn("Restore-AuthoringExtension");

Task("Build")
  .IsDependentOn("Build-Sdk")
  .IsDependentOn("Build-AuthoringExtension");

Task("Test")
  .IsDependentOn("Test-Sdk");

Task("Default")
  .IsDependentOn("Restore")
  .IsDependentOn("Build")
  .IsDependentOn("Test")
  .IsDependentOn("Publish-To-Folder")
  .IsDependentOn("Deploy-To-irAuthor");

Task("CI")
  .IsDependentOn("Clean")
  .IsDependentOn("Restore")
  .IsDependentOn("Build")
  .IsDependentOn("Test")
  .IsDependentOn("Publish-To-Folder");

Task("Release")
  .IsDependentOn("Clean")
  .IsDependentOn("Restore")
  .IsDependentOn("Build")
  .IsDependentOn("Test")
  .IsDependentOn("Publish-To-GitHub")
  .IsDependentOn("Publish-To-NuGet-Feed");*/

Task("Default")
  .IsDependentOn("Test");

Task("GitHub")
  .IsDependentOn("Clean")
  .IsDependentOn("Test");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(Argument("target", "Default"));