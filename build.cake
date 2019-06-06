// Install addins.
#addin nuget:?package=Cake.Incubator&version=5.0.1
#addin nuget:?package=Cake.OctoDeploy&version=3.2.0

// Install tools.
#tool nuget:?package=GitVersion.CommandLine&version=5.0.0-beta3-31

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
const string solutionFile = "./InRuleContribGit.sln";

//////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
//////////////////////////////////////////////////////////////////////

Setup(context =>
{
  context.Information("Calculating GitVersion...");

  gitVersion = GitVersion(new GitVersionSettings
  {
    NoFetch = true,
    UpdateAssemblyInfo = false
  });

  context.Information("Result:\n{0}", gitVersion.Dump());
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
    MSBuild("./src/InRuleContrib.Authoring.Extensions.Git/InRuleContrib.Authoring.Extensions.Git.csproj", new MSBuildSettings 
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
    NuGetRestore("./src/InRuleContrib.Authoring.Extensions.Git/InRuleContrib.Authoring.Extensions.Git.csproj", new NuGetRestoreSettings
    { 
      PackagesDirectory = "./packages"
    });
  });

Task("Build")
  .IsDependentOn("Restore")
  .Does(() => 
  {
    DotNetCoreBuild("./src/InRuleContrib.Repository.Storage.Git/InRuleContrib.Repository.Storage.Git.csproj", new DotNetCoreBuildSettings
    {
      Configuration = configuration,
      NoRestore = true
    });

    DotNetCoreBuild("./test/InRuleContrib.Repository.Storage.Git.Tests/InRuleContrib.Repository.Storage.Git.Tests.csproj", new DotNetCoreBuildSettings
    {
      Configuration = configuration,
      NoRestore = true
    });

    MSBuild("./src/InRuleContrib.Authoring.Extensions.Git/InRuleContrib.Authoring.Extensions.Git.csproj", new MSBuildSettings
    {
      Configuration = configuration,
      Restore = false,
      ToolVersion = MSBuildToolVersion.VS2019
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
    
    CopyFiles($"./src/**/{configuration}/**/*.{gitVersion.NuGetVersion}.nupkg", artifactsFolder);
    CopyFiles($"./src/**/{configuration}/**/*.{gitVersion.NuGetVersion}.symbols.nupkg", artifactsFolder);

    if (!DirectoryExists($"{artifactsFolder}/InRuleContrib.Authoring.Extensions.Git")) { CreateDirectory($"{artifactsFolder}/InRuleContrib.Authoring.Extensions.Git"); }
    if (!DirectoryExists($"{artifactsFolder}/InRuleContrib.Authoring.Extensions.Git/lib")) { CreateDirectory($"{artifactsFolder}/InRuleContrib.Authoring.Extensions.Git/lib"); }
    if (!DirectoryExists($"{artifactsFolder}/InRuleContrib.Authoring.Extensions.Git/lib/win32")) { CreateDirectory($"{artifactsFolder}/InRuleContrib.Authoring.Extensions.Git/lib/win32"); }

    CopyFiles($"./src/InRuleContrib.Authoring.Extensions.Git/bin/{configuration}/InRuleContrib.Authoring.Extensions.Git.*", $"{artifactsFolder}/InRuleContrib.Authoring.Extensions.Git/");
    CopyFiles($"./src/InRuleContrib.Authoring.Extensions.Git/bin/{configuration}/InRuleContrib.Repository.Storage.Git.*", $"{artifactsFolder}/InRuleContrib.Authoring.Extensions.Git/");
    
    CopyFiles($"./src/InRuleContrib.Authoring.Extensions.Git/bin/{configuration}/LibGit2Sharp.*", $"{artifactsFolder}/InRuleContrib.Authoring.Extensions.Git/");
    CopyFiles($"./src/InRuleContrib.Authoring.Extensions.Git/bin/{configuration}/lib/win32/**/*", $"{artifactsFolder}/InRuleContrib.Authoring.Extensions.Git/lib/win32/");
    DeleteFiles($"{artifactsFolder}/InRuleContrib.Authoring.Extensions.Git/**/*.xml");
    
    Zip($"{artifactsFolder}/InRuleContrib.Authoring.Extensions.Git/", $"{artifactsFolder}/InRuleContrib.Authoring.Extensions.Git.{gitVersion.NuGetVersion}.zip");
    
    DeleteDirectory($"{artifactsFolder}/InRuleContrib.Authoring.Extensions.Git/", new DeleteDirectorySettings
    {
      Force = true,
      Recursive = true,
    });
  });

Task("Deploy-To-irAuthor")
  .IsDependentOn("Publish-To-Folder")
  .Does(() =>
  {
    var extensionFolder = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/InRule/irAuthor/ExtensionExchange/InRuleContrib.Authoring.Extensions.Git";

    if (DirectoryExists(extensionFolder)) { DeleteDirectory(extensionFolder, new DeleteDirectorySettings { Force = true, Recursive = true }); }
    CreateDirectory(extensionFolder);

    Unzip($"{artifactsFolder}/InRuleContrib.Authoring.Extensions.Git.{gitVersion.NuGetVersion}.zip", extensionFolder);
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
        Repository = "inrule-contrib-git"
      }
    );
  });

Task("Publish-To-MyGet-Feed")
  .IsDependentOn("Publish-To-Folder")
  .Does(() =>
  {
    if (string.IsNullOrWhiteSpace(nugetPublishSource))
    {
      throw new InvalidOperationException("Cannot publish NuGet package(s) to the MyGet feed. You must provide a NuGet publish url via the 'nugetPublishSource' command-line argument or the 'NuGet_Publish_Source' environment variable.");
    }

    Func<IFileSystemInfo, bool> excludeSymbolPackages =
      fileSystemInfo => !fileSystemInfo.Path.FullPath.EndsWith(".symbols.nupkg", StringComparison.OrdinalIgnoreCase);

    NuGetPush(GetFiles("./artifacts/**/*.nupkg", excludeSymbolPackages), new NuGetPushSettings {
      Source = nugetPublishSource,
    });
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