public class BuildVersion
{
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
      //UpdateAssemblyInfo = true,
      //UpdateAssemblyInfoFilePath = "./src/Sknet.InRuleGitStorage.AuthoringExtension/Properties/AssemblyInfo.cs"
    });

    context.Information("Result:\n{0}", gitVersion.Dump());
  }
}