using InRule.Repository;
using LibGit2Sharp;
using System;
using System.Runtime.CompilerServices;

namespace Sknet.InRuleGitStorage
{
    /// <summary>
    /// Represents the rule application information for a specific commit.
    /// </summary>
    public class RuleApplicationGitInfo
    {
        /// <summary>
        /// Get the unique identifier of the rule application.
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// Gets the name of the rule application.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the description of the rule application.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Gets the active state of the rule application.
        /// </summary>
        public bool IsActive { get; }

        /// <summary>
        /// Get the related commit information of the rule application.
        /// </summary>
        public RuleApplicationGitCommitInfo Commit { get; } 

        internal RuleApplicationGitInfo(RuleApplicationDef ruleApplication, Commit commit)
        {
            if (ruleApplication == null)
            {
                throw new ArgumentNullException(nameof(ruleApplication));
            }

            if (commit == null)
            {
                throw new ArgumentNullException(nameof(commit));
            }

            Guid = ruleApplication.Guid;
            Name = ruleApplication.Name;
            Description = ruleApplication.Comments;
            IsActive = ruleApplication.IsActive;
            Commit = new RuleApplicationGitCommitInfo(commit);
        }
    }
}
