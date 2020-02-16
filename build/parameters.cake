#load "./directories.cake"
#load "./files.cake"
#load "./version.cake"

public class BuildParameters
{
  public string Configuration { get; }
  public BuildDirectories Directories { get; }
  public BuildFiles Files { get; }
  public GitHub GitHub { get; }
  public InRule InRule { get; }
  public bool IsLocalBuild { get; }
  public bool IsRunningOnUnix { get; }
  public bool IsRunningOnWindows { get; }
  public NuGet NuGet { get; }
  public string Target { get; }
  public BuildVersion Version { get; }

  public BuildParameters(ISetupContext context)
  {
    if (context == null)
    {
      throw new ArgumentNullException(nameof(context));
    }

    var buildSystem = context.BuildSystem();

    Configuration = context.Argument("configuration", "Release");
    GitHub = new GitHub(context);
    InRule = new InRule(context);
    IsLocalBuild = buildSystem.IsLocalBuild;
    IsRunningOnUnix = context.IsRunningOnUnix();
    IsRunningOnWindows = context.IsRunningOnWindows();   
    NuGet = new NuGet(context);
    Target = context.TargetTask.Name;
    Version = new BuildVersion(context);

    Directories = new BuildDirectories(context);
    Files = new BuildFiles(Directories);
  }
}

public class GitHub
{
  public string AccessToken { get; }

  public GitHub(ICakeContext context)
  {
    if (context == null)
    {
      throw new ArgumentNullException(nameof(context));
    }

    AccessToken = context.Argument("githubAccessToken", context.EnvironmentVariable("GitHub_Access_Token") ?? null);
  }
}

public class InRule
{
  public const string DefaultVersion = "5.2.0";
  public string Version { get; }

  public InRule(ICakeContext context)
  {
    if (context == null)
    {
      throw new ArgumentNullException(nameof(context));
    }

    Version = context.Argument("inRuleVersion", context.EnvironmentVariable("InRule_Version") ?? DefaultVersion);
  }
}

public class NuGet
{
  public string ApiKey { get; }

  public NuGet(ICakeContext context)
  {
    if (context == null)
    {
      throw new ArgumentNullException(nameof(context));
    }

    ApiKey = context.Argument("nugetApiKey", context.EnvironmentVariable("NuGet_ApiKey") ?? null);
  }
}

  /*GitVersion gitVersion;
const string artifactsFolder = "./artifacts";
const string solutionFolder = "./";
const string solutionFile = "./Sknet.InRuleGitStorage.sln";

string fullSemVer;
string assemblySemVer;
string informationalVersion;*/

/*Setup(context =>
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
});*/