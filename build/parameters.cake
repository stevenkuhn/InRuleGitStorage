
public class BuildParameters
{
  public string Configuration { get; }
  public GitHub GitHub { get; }
  public InRule InRule { get; }
  public NuGet NuGet { get; }
  public Paths Paths { get; }
  public string Target { get; }
  public Version Version { get; }

  public BuildParameters(ISetupContext context)
  {
    if (context == null)
    {
      throw new ArgumentNullException(nameof(context));
    }

    Configuration = context.Argument("configuration", "Release");
    GitHub = new GitHub(context);
    InRule = new InRule(context);
    NuGet = new NuGet(context);
    Paths = new Paths(context);
    Target = context.TargetTask.Name;
    Version = new Version(context);
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

public class Version
{
  public bool SkipGitVersion { get; }

  public Version(ICakeContext context)
  {
    if (context == null)
    {
      throw new ArgumentNullException(nameof(context));
    }

    context.Information("Calculating GitVersion...");

    var gitVersion = context.GitVersion(new GitVersionSettings
    {
      NoFetch = true,
      //UpdateAssemblyInfo = true,
      //UpdateAssemblyInfoFilePath = "./src/Sknet.InRuleGitStorage.AuthoringExtension/Properties/AssemblyInfo.cs"
    });

    context.Information("Result:\n{0}", gitVersion.Dump());
  }
}

public class Paths
{
  public Directories Directories { get; }
  public Files Files { get; }
  
  public Paths(ICakeContext context)
  {
    if (context == null)
    {
      throw new ArgumentNullException(nameof(context));
    }

    Directories = new Directories(context);
    Files = new Files(Directories);
  }
} 

public class Files
{
  public string AuthoringProject { get; }
  public string SdkProject { get; }
  public string SdkTestProject { get; }
  public string Solution { get; }

  public Files(Directories dirs)
  {
    if (dirs == null)
    {
      throw new ArgumentNullException(nameof(dirs));
    }

    AuthoringProject = dirs.Source.CombineWithFilePath("Sknet.InRuleGitStorage.AuthoringExtension/Sknet.InRuleGitStorage.AuthoringExtension.csproj").FullPath;
    SdkProject = dirs.Source.CombineWithFilePath("Sknet.InRuleGitStorage/Sknet.InRuleGitStorage.csproj").FullPath;
    SdkTestProject = dirs.Test.CombineWithFilePath("Sknet.InRuleGitStorage.Tests/Sknet.InRuleGitStorage.Tests.csproj").FullPath;
    Solution = dirs.Solution.CombineWithFilePath("Sknet.InRuleGitStorage.sln").FullPath;
  }
}

public class Directories
{
  public DirectoryPath Artifacts { get; } = "./artifacts";
  public DirectoryPath Solution { get; } = "./";
  public DirectoryPath Source { get; } = "./src";
  public DirectoryPath Test { get; } = "./test";
  public DirectoryPathCollection TestResults { get; }

  public Directories(ICakeContext context)
  {
    if (context == null)
    {
      throw new ArgumentNullException(nameof(context));
    }

    TestResults = context.GetDirectories(Test.Combine("**/TestResults").ToString());
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