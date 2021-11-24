namespace Sknet.InRuleGitStorage;

/// <summary>
/// Represents extensions for the primary interface for storing and managing
/// InRule rule applications in a git repository.
/// </summary>
public static class IInRuleGitRepositoryExtensions
{
    /// <summary>
    /// Store the content of the specified rule application in the current
    /// branch as a new commit.
    /// </summary>
    /// <param name="repository">The Git repository instance.</param>
    /// <param name="ruleApplication">The rule application to store in the repository.</param>
    /// <param name="message">The description of why a change was made to the repository.</param>
    /// <returns>The generated commit containing the specified rule application and any existing rule applications.</returns>
    public static Commit Commit(this IInRuleGitRepository repository, RuleApplicationDef ruleApplication, string message)
    {
        if (repository == null) throw new ArgumentNullException(nameof(repository));

        var signature = repository.Config.BuildSignature(DateTimeOffset.Now);

        return repository.Commit(ruleApplication, message, signature, signature);
    }

    /// <summary>
    /// Fetch all of the latest changes from a remote InRule git repository.
    /// </summary>
    /// <param name="repository">The Git repository instance.</param>
    public static void Fetch(this IInRuleGitRepository repository)
    {
        if (repository == null) throw new ArgumentNullException(nameof(repository));

        repository.Fetch(new FetchOptions());
    }

    /// <summary>
    /// Fetch all of the latest changes from a remote InRule git repository.
    /// </summary>
    /// <param name="repository">The Git repository instance.</param>
    /// <param name="options">The parameters that control the fetch behavior.</param>
    public static void Fetch(this IInRuleGitRepository repository, FetchOptions options)
    {
        if (repository == null) throw new ArgumentNullException(nameof(repository));

        repository.Fetch("origin", options);
    }

    /// <summary>
    /// Perform a merge of the current branch and the specified branch, and
    /// create a commit if there are no conflicts.
    /// </summary>
    /// <param name="repository">The Git repository instance.</param>
    /// <param name="branchName">The branch name to merge with the current branch.</param>
    /// <returns>The result of a merge of two trees and any conflicts.</returns>
    public static MergeTreeResult Merge(this IInRuleGitRepository repository, string branchName)
    {
        if (repository == null) throw new ArgumentNullException(nameof(repository));

        return repository.Merge(branchName, new MergeOptions());
    }

    /// <summary>
    /// Perform a merge of the current branch and the specified branch, and
    /// create a commit if there are no conflicts.
    /// </summary>
    /// <param name="repository">The Git repository instance.</param>
    /// <param name="branchName">The branch name to merge with the current branch.</param>
    /// <param name="options">The parameters that control the merge behavior.</param>
    /// <returns>The result of a merge of two trees and any conflicts.</returns>
    public static MergeTreeResult Merge(this IInRuleGitRepository repository, string branchName, MergeOptions options)
    {
        if (repository == null) throw new ArgumentNullException(nameof(repository));

        var signature = repository.Config.BuildSignature(DateTimeOffset.Now);

        return repository.Merge(branchName, signature, options);
    }

    /// <summary>
    /// Fetch all of the changes from a remote InRule git repository and
    /// merge into the current branch.
    /// </summary>
    /// <param name="repository">The Git repository instance.</param>
    /// <returns>The result of a merge of two trees and any conflicts.</returns>
    public static MergeTreeResult Pull(this IInRuleGitRepository repository)
    {
        if (repository == null) throw new ArgumentNullException(nameof(repository));

        return repository.Pull(new PullOptions());
    }

    /// <summary>
    /// Fetch all of the changes from a remote InRule git repository and
    /// merge into the current branch.
    /// </summary>
    /// <param name="repository">The Git repository instance.</param>
    /// <param name="options">The parameters that control the fetch and merge behavior.</param>
    /// <returns>The result of a merge of two trees and any conflicts.</returns>
    public static MergeTreeResult Pull(this IInRuleGitRepository repository, PullOptions options)
    {
        if (repository == null) throw new ArgumentNullException(nameof(repository));

        var signature = repository.Config.BuildSignature(DateTimeOffset.Now);

        return repository.Pull(signature, options);
    }

    /// <summary>
    /// Fetch all of the changes from a remote InRule git repository and
    /// merge into the current branch.
    /// </summary>
    /// <param name="repository">The Git repository instance.</param>
    /// <param name="merger">The signature to use for the merge.</param>
    /// <param name="options">The parameters that control the fetch and merge behavior.</param>
    /// <returns>The result of a merge of two trees and any conflicts.</returns>
    public static MergeTreeResult Pull(this IInRuleGitRepository repository, Signature merger, PullOptions options)
    {
        if (repository == null) throw new ArgumentNullException(nameof(repository));

        return repository.Pull("origin", merger, options);
    }

    /// <summary>
    /// Fetch all of the changes from a remote InRule git repository and
    /// merge into the current branch.
    /// </summary>
    /// <param name="repository">The Git repository instance.</param>
    /// <param name="remote">The name or URI for the remote repository.</param>
    /// <param name="options">The parameters that control the fetch and merge behavior.</param>
    /// <returns>The result of a merge of two trees and any conflicts.</returns>
    public static MergeTreeResult Pull(this IInRuleGitRepository repository, string remote, PullOptions options)
    {
        if (repository == null) throw new ArgumentNullException(nameof(repository));

        var signature = repository.Config.BuildSignature(DateTimeOffset.Now);

        return repository.Pull(remote, signature, options);
    }

    /// <summary>
    /// Push the current branch to a remote InRule git repository.
    /// </summary>
    /// <param name="repository">The Git repository instance.</param>
    public static void Push(this IInRuleGitRepository repository)
    {
        if (repository == null) throw new ArgumentNullException(nameof(repository));

        repository.Push("origin", new PushOptions());
    }

    /// <summary>
    /// Push the current branch to a remote InRule git repository.
    /// </summary>
    /// <param name="repository">The Git repository instance.</param>
    /// <param name="options">The parameters that control the push behavior.</param>
    public static void Push(this IInRuleGitRepository repository, PushOptions options)
    {
        if (repository == null) throw new ArgumentNullException(nameof(repository));

        repository.Push("origin", options);
    }

    /// <summary>
    ///  Create a commit that removes the specified rule application from the current branch.
    /// </summary>
    /// <param name="repository">The Git repository instance.</param>
    /// <param name="ruleApplicationName">The case-insensitive rule application name.</param>
    /// <param name="message">The description of why a change was made to the repository.</param>
    public static Commit? RemoveRuleApplication(this IInRuleGitRepository repository, string ruleApplicationName, string message)
    {
        if (ruleApplicationName == null) throw new ArgumentNullException(nameof(ruleApplicationName));
        if (string.IsNullOrWhiteSpace(ruleApplicationName)) throw new ArgumentException("Specified rule application name cannot be null or whitespace.", nameof(ruleApplicationName));
        if (message == null) throw new ArgumentNullException(nameof(message));

        var signature = repository.Config.BuildSignature(DateTimeOffset.Now);

        return repository.RemoveRuleApplication(ruleApplicationName, message, signature, signature);
    }
}
