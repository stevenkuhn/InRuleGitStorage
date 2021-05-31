using LibGit2Sharp;
using System;

namespace Sknet.InRuleGitStorage
{
    /// <summary>
    /// Represents the signature information in an InRule git repository.
    /// </summary>
    public class RuleApplicationGitSignatureInfo
    {
        /// <summary>
        /// Gets the name of the signature.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Gets the name of the signature
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the date when this signature happened.
        /// </summary>
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
