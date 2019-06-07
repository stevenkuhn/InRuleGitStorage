using InRule.Repository;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("InRuleContrib.Repository.Storage.Git.Tests")]

namespace InRuleContrib.Repository.Storage.Git
{
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
        /// Create a new branch from the current branch.
        /// </summary>
        /// <param name="branchName">The branch name for the new branch.</param>
        /// <returns>The created branch.</returns>
        Branch CreateBranch(string branchName);

        /// <summary>
        /// Get a rule application from the current branch.
        /// </summary>
        /// <param name="ruleApplicationName">The case-insensitive rule application name.</param>
        /// <returns>A rule application found in the current branch that has the specified
        /// name; null otherwise.</returns>
        RuleApplicationDef GetRuleApplication(string ruleApplicationName);

        /// <summary>
        /// Remove an existing branch.
        /// </summary>
        /// <param name="branchName">The branch name to remove.</param>
        void RemoveBranch(string branchName);
    }
}
