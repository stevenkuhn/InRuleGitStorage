public class BuildVersion
{
  public string AssemblySemanticVersion { get; }
  public string FullSemanticVersion { get; }
  public string InformationalVersion { get; }
  public string SemanticVersion { get; }
  public bool SkipGitVersion { get; }
  
  public BuildVersion(ICakeContext context)
  {
    if (context == null)
    {
      throw new ArgumentNullException(nameof(context));
    }

    context.Information("Calculating GitVersion...");

    var gitVersion = context.GitVersion(new GitVersionSettings
    {
      NoFetch = true,
      UpdateAssemblyInfo = true,
      UpdateAssemblyInfoFilePath = "./src/Sknet.InRuleGitStorage.AuthoringExtension/Properties/AssemblyInfo.cs"
    });

    SemanticVersion = gitVersion.SemVer;
    FullSemanticVersion = gitVersion.FullSemVer;
    AssemblySemanticVersion = gitVersion.AssemblySemVer;
    InformationalVersion = gitVersion.InformationalVersion;
  }
}