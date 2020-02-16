public class BuildDirectories
{
  public DirectoryPath Artifacts { get; } = "./artifacts";
  public DirectoryPath Solution { get; } = "./";
  public DirectoryPath Source { get; } = "./src";
  public DirectoryPath Test { get; } = "./test";
  public DirectoryPathCollection TestResults { get; }

  public BuildDirectories(ICakeContext context)
  {
    if (context == null)
    {
      throw new ArgumentNullException(nameof(context));
    }

    TestResults = context.GetDirectories(Test.Combine("**/TestResults").ToString());
  }
}