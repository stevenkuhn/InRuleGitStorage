[assembly: InternalsVisibleTo("Sknet.InRuleGitStorage.Tests")]

namespace Sknet.InRuleGitStorage;

/// <summary>
/// Represents the primary interface for storing and managing InRule rule
/// applications in a git repository.
/// </summary>
public interface IInRuleGitRepository : IDisposable
{
    /// <summary>
    /// Switch the current branch to the specified branch name.
    /// </summary>
    /// <param name="branchName">The branch name to switch to.</param>
    void Checkout(string branchName);

    /// <summary>
    /// Store the content of the specified rule application in the current
    /// branch as a new commit.
    /// </summary>
    /// <param name="ruleApplication">The rule application to store in the repository.</param>
    /// <param name="message">The description of why a change was made to the repository.</param>
    /// <param name="author">The signature of who made the change.</param>
    /// <param name="committer">The signature of who added the change to the repository.</param>
    /// <returns>The generated commit containing the specified rule application and any existing rule applications.</returns>
    Commit Commit(RuleApplicationDef ruleApplication, string message, Signature author, Signature committer);

    /// <summary>
    /// Provides access to the configuration settings for this repository.
    /// </summary>
    Configuration Config { get; }

    /// <summary>
    /// Create a new branch from the current branch.
    /// </summary>
    /// <param name="branchName">The branch name for the new branch.</param>
    /// <returns>The created branch.</returns>
    Branch CreateBranch(string branchName);

    /// <summary>
    /// Create a new tracked branch from the remote branch of the same name.
    /// </summary>
    /// <param name="branchName">The branch name for the new branch.</param>
    /// <param name="remote">The name for the remote repository.</param>
    /// <returns>The created branch.</returns>
    Branch CreateBranch(string branchName, string remote);

    /// <summary>
    /// Fetch all of the latest changes from a remote InRule git repository.
    /// </summary>
    /// <param name="remote">The name or URI for the remote repository.</param>
    /// <param name="options">The parameters that control the fetch behavior.></param>
    void Fetch(string remote, FetchOptions options);

    /// <summary>
    /// Get a rule application from the current branch.
    /// </summary>
    /// <param name="ruleApplicationName">The case-insensitive rule application name.</param>
    /// <returns>A rule application found in the current branch that has the specified
    /// name; null otherwise.</returns>
    RuleApplicationDef? GetRuleApplication(string ruleApplicationName);

    /// <summary>
    /// Get a collection of references to available rule application from the current branch.
    /// </summary>
    /// <returns>A collection of rule application references (not the actual rule applications) from the current branch.</returns>
    RuleApplicationGitInfo[] GetRuleApplications();

    /// <summary>
    /// Perform a merge of the current branch and the specified branch, and
    /// create a commit if there are no conflicts.
    /// </summary>
    /// <param name="branchName">The branch name to merge with the current branch.</param>
    /// <param name="merger">The signature to use for the merge.</param>
    /// <param name="options">The parameters that control the merge behavior.</param>
    /// <returns>The result of a merge of two trees and any conflicts.</returns>
    MergeTreeResult Merge(string branchName, Signature merger, MergeOptions options);

    /// <summary>
    /// Fetch all of the changes from a remote InRule git repository and
    /// merge into the current branch.
    /// </summary>
    /// <param name="remote">The name or URI for the remote repository.</param>
    /// <param name="merger">The signature to use for the merge.</param>
    /// <param name="options">The parameters that control the fetch and merge behavior.</param>
    /// <returns>The result of a merge of two trees and any conflicts.</returns>
    MergeTreeResult Pull(string remote, Signature merger, PullOptions options);

    /// <summary>
    /// Push the current branch to a remote InRule git repository.
    /// </summary>
    /// <param name="remote">The name or URI for the remote repository.</param>
    /// <param name="options">The parameters that control the push behavior.</param>
    void Push(string remote, PushOptions options);

    /// <summary>
    /// Lookup and manage remotes in the repository.
    /// </summary>
    RemoteCollection Remotes { get; }

    /// <summary>
    /// Remove an existing branch.
    /// </summary>
    /// <param name="branchName">The branch name to remove.</param>
    void RemoveBranch(string branchName);

    /// <summary>
    /// Create a commit that removes the specified rule application from the current branch.
    /// </summary>
    /// <param name="ruleApplicationName">The case-insensitive rule application name.</param>
    /// <param name="message">The description of why a change was made to the repository.</param>
    /// <param name="author">The signature of who made the change.</param>
    /// <param name="committer">The signature of who added the change to the repository.</param>
    Commit? RemoveRuleApplication(string ruleApplicationName, string message, Signature author, Signature committer);
}