using InRule.Repository;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sknet.InRuleGitStorage.Extensions
{
    internal static class CommitExtensions
    {
        internal static RuleApplicationDef GetRuleApplication(this Commit commit, string ruleApplicationName)
        {
            if (commit == null) throw new ArgumentNullException(nameof(commit));
            if (string.IsNullOrWhiteSpace(ruleApplicationName)) throw new ArgumentException("Specified rule application name cannot be null or whitespace.", nameof(ruleApplicationName));

            var ruleAppTreeEntry = commit.Tree[ruleApplicationName];

            if (ruleAppTreeEntry?.Target == null || !(ruleAppTreeEntry.Target is Tree))
            {
                return null;
            }

            IInRuleGitSerializer serializer = new InRuleGitSerializer();
            return serializer.Deserialize(ruleAppTreeEntry);
        }
    }
}
