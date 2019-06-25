using InRule.Repository;
using LibGit2Sharp;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("InRuleContrib.Repository.Storage.Git.Tests")]

namespace InRuleContrib.Repository.Storage.Git
{
    public class RuleApplicationGitInfo
    {
        public Guid Guid { get; }
        public string Name { get; }
        public string Description { get; }
        public bool IsActive { get; }

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
