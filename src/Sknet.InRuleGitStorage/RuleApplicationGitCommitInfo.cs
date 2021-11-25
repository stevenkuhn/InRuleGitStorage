namespace Sknet.InRuleGitStorage;

/// <summary>
/// Represents the commit information in a InRule git repository.
/// </summary>
public class RuleApplicationGitCommitInfo
{
    /// <summary>
    /// Gets the 40 character sha1 of the commit.
    /// </summary>
    public string Sha { get; }

    /// <summary>
    /// Gets the message of the commit.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the short commit message which is usually the first line of the commit.
    /// </summary>
    public string MessageShort { get; }

    /// <summary>
    /// Gets the author of the commit.
    /// </summary>
    public RuleApplicationGitSignatureInfo Author { get; }

    /// <summary>
    /// Gets the committer of the commit.
    /// </summary>
    public RuleApplicationGitSignatureInfo Committer { get; }

    internal RuleApplicationGitCommitInfo(Commit commit)
    {
        if (commit == null)
        {
            throw new ArgumentNullException(nameof(commit));
        }

        Sha = commit.Sha;
        Message = commit.Message;
        MessageShort = commit.MessageShort;
        Author = new RuleApplicationGitSignatureInfo(commit.Author);
        Committer = new RuleApplicationGitSignatureInfo(commit.Committer);
    }
}