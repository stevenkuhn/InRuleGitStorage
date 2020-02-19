#load "./directories.cake"

public class BuildFiles
{
  public string AuthoringProject { get; }
  public string SdkProject { get; }
  public string SdkTestProject { get; }
  public string Solution { get; }

  public BuildFiles(ICakeContext context, BuildDirectories dirs)
  {
    if (context == null)
    {
      throw new ArgumentNullException(nameof(dirs));
    }

    if (dirs == null)
    {
      throw new ArgumentNullException(nameof(dirs));
    }

    AuthoringProject = context.MakeAbsolute(dirs.Source.CombineWithFilePath("Sknet.InRuleGitStorage.AuthoringExtension/Sknet.InRuleGitStorage.AuthoringExtension.csproj")).FullPath;
    SdkProject = context.MakeAbsolute(dirs.Source.CombineWithFilePath("Sknet.InRuleGitStorage/Sknet.InRuleGitStorage.csproj")).FullPath;
    SdkTestProject = context.MakeAbsolute(dirs.Test.CombineWithFilePath("Sknet.InRuleGitStorage.Tests/Sknet.InRuleGitStorage.Tests.csproj")).FullPath;
    Solution = context.MakeAbsolute(dirs.Solution.CombineWithFilePath("Sknet.InRuleGitStorage.sln")).FullPath;
  }
}