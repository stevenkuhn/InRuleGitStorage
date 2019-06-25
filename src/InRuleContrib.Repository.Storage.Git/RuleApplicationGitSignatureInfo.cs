using LibGit2Sharp;
using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("InRuleContrib.Repository.Storage.Git.Tests")]

namespace InRuleContrib.Repository.Storage.Git
{
    public class RuleApplicationGitSignatureInfo
    {
        public string Email { get; }
        public string Name { get; }
        public DateTimeOffset When { get; }

        internal RuleApplicationGitSignatureInfo(Signature signature)
        {
            if (signature == null)
            {
                throw new ArgumentNullException(nameof(signature));
            }

            Email = signature.Email;
            Name = signature.Name;
            When = signature.When;
        }
    }
}
