// Install addins.
#addin nuget:?package=Cake.Incubator&version=5.1.0
#addin nuget:?package=Cake.OctoDeploy&version=3.2.0

// Install tools.
#tool nuget:?package=GitVersion.CommandLine&version=5.1.2

//////////////////////////////////////////////////////////////////////
// PARAMETERS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");
var githubAccessToken = Argument("githubAccessToken", EnvironmentVariable("GitHub_Access_Token") ?? null);
var nugetPublishSource = Argument("nugetPublishSource", EnvironmentVariable("NuGet_Publish_Source") ?? null);

//////////////////////////////////////////////////////////////////////
// GLOBALS
//////////////////////////////////////////////////////////////////////
GitVersion gitVersion;
const string artifactsFolder = "./artifacts";
const string solutionFolder = "./";
const string solutionFile = "./Sknet.InRuleGitStorage.sln";

string fullSemVer;
string assemblySemVer;
string informationalVersion;

//////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
//////////////////////////////////////////////////////////////////////

Setup(context =>
{
  context.Information("Calculating GitVersion...");

  gitVersion = GitVersion(new GitVersionSettings
  {
    NoFetch = true,
    UpdateAssemblyInfo = true,
    UpdateAssemblyInfoFilePath = "./src/Sknet.InRuleGitStorage.AuthoringExtension/Properties/AssemblyInfo.cs"
  });

  context.Information("Result:\n{0}", gitVersion.Dump());

  fullSemVer = gitVersion.FullSemVer;
  assemblySemVer = gitVersion.AssemblySemVer;
  informationalVersion = gitVersion.InformationalVersion;
});

TaskSetup(context =>
{

});

TaskTeardown(context =>
{

});

Teardown(context => 
{

});

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Clean")
  .Does(() => 
  {
    MSBuild("./src/Sknet.InRuleGitStorage.AuthoringExtension/Sknet.InRuleGitStorage.AuthoringExtension.csproj", new MSBuildSettings 
    { 
      Configuration = configuration,
      Targets = { "Clean" },
      ToolVersion = MSBuildToolVersion.VS2019
    });

    DotNetCoreClean(solutionFolder, new DotNetCoreCleanSettings { Configuration = configuration });
  });

Task("Clean-Artifacts")
  .Does(() => 
  {
    if (DirectoryExists(artifactsFolder))
    {
      DeleteDirectory(artifactsFolder, new DeleteDirectorySettings { Force = true, Recursive = true });
    }
  });

Task("Clean-TestResults")
  .Does(() => 
  {
    var directories = GetDirectories("./test/**/TestResults");
    foreach (var directory in directories)
    {
      DeleteDirectory(directory, new DeleteDirectorySettings { Force = true, Recursive = true });
    }
  });

Task("Restore")
  .Does(() => 
  {
    NuGetRestore("./src/Sknet.InRuleGitStorage.AuthoringExtension/Sknet.InRuleGitStorage.AuthoringExtension.csproj", new NuGetRestoreSettings
    { 
      PackagesDirectory = "./packages"
    });
  });

Task("Build")
  .IsDependentOn("Restore")
  .Does(() => 
  {
    DotNetCoreBuild("./src/Sknet.InRuleGitStorage/Sknet.InRuleGitStorage.csproj", new DotNetCoreBuildSettings
    {
      ArgumentCustomization = args => args.Append($"/p:Version={fullSemVer}")
                                          .Append($"/p:AssemblyVersion={assemblySemVer}")
                                          .Append($"/p:InformationalVersion={informationalVersion}"),
      Configuration = configuration
    });

    DotNetCoreBuild("./test/Sknet.InRuleGitStorage.Tests/Sknet.InRuleGitStorage.Tests.csproj", new DotNetCoreBuildSettings
    {
      ArgumentCustomization = args => args.Append($"/p:Version={fullSemVer}")
                                          .Append($"/p:AssemblyVersion={assemblySemVer}")
                                          .Append($"/p:InformationalVersion={informationalVersion}"),
      Configuration = configuration
    });

    MSBuild("./src/Sknet.InRuleGitStorage.AuthoringExtension/Sknet.InRuleGitStorage.AuthoringExtension.csproj", new MSBuildSettings
    {
      ArgumentCustomization = args => args.Append($"/p:Version={fullSemVer}")
                                          .Append($"/p:AssemblyVersion={assemblySemVer}")
                                          .Append($"/p:InformationalVersion={informationalVersion}"),
      Configuration = configuration,
      Restore = false,
      ToolVersion = MSBuildToolVersion.VS2019,
    });
  });

Task("Test")
  .IsDependentOn("Clean-TestResults")
  .IsDependentOn("Build")
  .Does(() => 
  {
    var projectFiles = GetFiles("./test/**/*.csproj");
    foreach (var file in projectFiles)
    {
      // Ignore any temporary nCrunch projects that might be running when this build runs
      if (file.FullPath.IndexOf("nCrunch", StringComparison.OrdinalIgnoreCase) >= 0) continue;

      DotNetCoreTest(file.FullPath, new DotNetCoreTestSettings
      {
        Configuration = configuration,
        Framework = "netcoreapp2.2",
        NoBuild = true,
        NoRestore = true,
        Logger = "trx;LogFileName=./netcoreapp2.2/TestResult.trx",
      });

      DotNetCoreTest(file.FullPath, new DotNetCoreTestSettings
      {
        Configuration = configuration,
        Framework = "net461",
        NoBuild = true,
        NoRestore = true,
        Logger = "trx;LogFileName=./net461/TestResult.trx"
      });
    }
  });

Task("Publish-To-Folder")
  .IsDependentOn("Clean-Artifacts")
  .IsDependentOn("Build")
  .Does(() =>
  {
    if (!DirectoryExists(artifactsFolder)) { CreateDirectory(artifactsFolder); }
    
    CopyFiles($"./src/**/{configuration}/**/*.{gitVersion.SemVer}.nupkg", artifactsFolder);
    CopyFiles($"./src/**/{configuration}/**/*.{gitVersion.SemVer}.symbols.nupkg", artifactsFolder);

    if (!DirectoryExists($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension")) { CreateDirectory($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension"); }
    if (!DirectoryExists($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/lib")) { CreateDirectory($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/lib"); }
    if (!DirectoryExists($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/lib/win32")) { CreateDirectory($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/lib/win32"); }

    CopyFiles($"./src/Sknet.InRuleGitStorage.AuthoringExtension/bin/{configuration}/Sknet.InRuleGitStorage.AuthoringExtension.*", $"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/");
    CopyFiles($"./src/Sknet.InRuleGitStorage.AuthoringExtension/bin/{configuration}/Sknet.InRuleGitStorage.*", $"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/");
    
    CopyFiles($"./src/Sknet.InRuleGitStorage.AuthoringExtension/bin/{configuration}/LibGit2Sharp.*", $"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/");
    CopyFiles($"./src/Sknet.InRuleGitStorage.AuthoringExtension/bin/{configuration}/lib/win32/**/*", $"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/lib/win32/");
    DeleteFiles($"{artifactsFolder}/Sknet.InRuleGitStorage.AuthoringExtension/**/*.xml");
    
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
    artifacts.AddRange(GetFiles("./artifacts/**/*.nupkg"));
    artifacts.AddRange(GetFiles("./artifacts/**/*.zip"));
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

Task("Publish-To-MyGet-Feed")
  .IsDependentOn("Publish-To-Folder")
  .Does(() =>
  {
    // if (string.IsNullOrWhiteSpace(nugetPublishSource))
    // {
    //   throw new InvalidOperationException("Cannot publish NuGet package(s) to the MyGet feed. You must provide a NuGet publish url via the 'nugetPublishSource' command-line argument or the 'NuGet_Publish_Source' environment variable.");
    // }

    // Func<IFileSystemInfo, bool> excludeSymbolPackages =
    //   fileSystemInfo => !fileSystemInfo.Path.FullPath.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase);

    // NuGetPush(GetFiles("./artifacts/**/*.nupkg", excludeSymbolPackages), new NuGetPushSettings {
    //   Source = nugetPublishSource,
    // });
  });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

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
  .IsDependentOn("Publish-To-MyGet-Feed");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);