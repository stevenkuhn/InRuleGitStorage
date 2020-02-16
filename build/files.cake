#load "./directories.cake"

public class BuildFiles
{
  public string AuthoringProject { get; }
  public string SdkProject { get; }
  public string SdkTestProject { get; }
  public string Solution { get; }

  public BuildFiles(BuildDirectories dirs)
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