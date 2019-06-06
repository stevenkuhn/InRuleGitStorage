// Install addins.
#addin nuget:?package=Cake.Incubator&version=5.0.1
#addin nuget:?package=Cake.OctoDeploy&version=3.2.0

// Install tools.
#tool nuget:?package=GitVersion.CommandLine&version=5.0.0-beta3-26

//////////////////////////////////////////////////////////////////////
// PARAMETERS
//////////////////////////////////////////////////////////////////////
var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// GLOBALS
//////////////////////////////////////////////////////////////////////
const string artifactsFolder = "./artifacts";
const string solutionFolder = "./";
const string solutionFile = "./InRuleContribGit.sln";
FilePath[] dotnetFrameworkProjects = new FilePath[]
{
  MakeAbsolute(new FilePath("./src/InRuleContrib.Authoring.Extensions.Git/InRuleContrib.Authoring.Extensions.Git.csproj"))
};
FilePath[] dotnetCoreProjects = GetFiles("./**/*.csproj")
                                  .Where(x => dotnetFrameworkProjects
                                  .All(y => y.FullPath != x.FullPath))
                                  .ToArray();      

//////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
//////////////////////////////////////////////////////////////////////

Setup(context =>
{
  context.Information("Calculating GitVersion...");

  GitVersion gitVersion = GitVersion(new GitVersionSettings
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
    foreach (var file in dotnetFrameworkProjects)
    {
      MSBuild(file.FullPath, new MSBuildSettings { Configuration = configuration, Targets = { "Clean" } });
    }

    foreach (var file in dotnetCoreProjects)
    {
      DotNetCoreClean(file.FullPath, new DotNetCoreCleanSettings { Configuration = configuration });
    }
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
    foreach (var file in dotnetFrameworkProjects)
    {
      NuGetRestore(file.FullPath,  new NuGetRestoreSettings { PackagesDirectory = "./packages" });
    }

    foreach (var file in dotnetCoreProjects)
    {
      DotNetCoreRestore(file.FullPath, new DotNetCoreRestoreSettings { });
    }
  });

Task("Build")
  .IsDependentOn("Restore")
  .Does(() => 
  {
    foreach (var file in dotnetFrameworkProjects)
    {
      MSBuild(file.FullPath, new MSBuildSettings
      {
        Configuration = configuration,
        Restore = false
      });
    }

    foreach (var file in dotnetCoreProjects)
    {
      DotNetCoreBuild(file.FullPath, new DotNetCoreBuildSettings
      {
        Configuration = configuration,
        NoRestore = true
      });
    }
  });

Task("Test")
  .IsDependentOn("Clean-TestResults")
  .IsDependentOn("Build")
  .Does(() => {
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

Task("PublishToFolder")
  .IsDependentOn("Clean-Artifacts")
  .IsDependentOn("Build")
  .Does(() => {

  });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
  .IsDependentOn("Clean")
  .IsDependentOn("Restore")
  .IsDependentOn("Build")
  .IsDependentOn("Test")
  .IsDependentOn("PublishToFolder");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);