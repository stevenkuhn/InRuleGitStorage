using LibGit2Sharp;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("InRuleContrib.Repository.Storage.Git.Tests")]

namespace InRuleContrib.Repository.Storage.Git
{
    public class RuleApplicationGitCommitInfo
    {
        public string Sha { get; }
        public string Message { get; }
        public string MessageShort { get; }
        public RuleApplicationGitSignatureInfo Author { get; }
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
}
