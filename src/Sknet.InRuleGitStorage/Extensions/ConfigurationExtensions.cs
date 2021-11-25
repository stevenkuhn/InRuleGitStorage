namespace LibGit2Sharp;

/// <summary>
/// 
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="config"></param>
    /// <returns></returns>
    public static string GetDefaultBranch(this Configuration config) =>
        config.GetValueOrDefault("init.defaultBranch", "master");
}
