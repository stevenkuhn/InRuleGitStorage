using InRule.Repository;
using InRuleContrib.Repository.Storage.Git.Extensions;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace InRuleContrib.Repository.Storage.Git
{
    /// <summary>
    /// Represents the primary interface for storing and managing InRule rule
    /// applications in a git repository.
    /// </summary>
    public class InRuleGitRepository : IInRuleGitRepository
    {
        private readonly IRepository _repository;

        internal InRuleGitRepository(string path)
        {
            _repository = new LibGit2Sharp.Repository(path);
        }

        internal InRuleGitRepository(IRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Switch the current branch to the specified branch name.
        /// </summary>
        /// <param name="branchName">The branch name to switch to.</param>
        public void Checkout(string branchName)
        {
            if (branchName == null) throw new ArgumentNullException(nameof(branchName));
            if (string.IsNullOrWhiteSpace(branchName)) throw new ArgumentException("Specified branch name cannot be null or whitespace.", nameof(branchName));

            var targetRef = _repository.Refs[$"refs/heads/{branchName}"];

            if (targetRef == null)
            {
                throw new ArgumentException("Specified branch name does not exist; cannot checkout.", nameof(branchName));
            }

            _repository.Refs.UpdateTarget(_repository.Refs.Head, targetRef);
        }

        /// <summary>
        /// Store the content of the specified rule application in the current
        /// branch as a new commit.
        /// </summary>
        /// <param name="ruleApplication">The rule application to store in the repository.</param>
        /// <param name="message">The description of why a change was made to the repository.</param>
        /// <param name="author">The signature of who made the change.</param>
        /// <param name="committer">The signature of who added the change to the repository.</param>
        /// <returns>The generated commit containing the specified rule application and any existing rule applications.</returns>
        public Commit Commit(RuleApplicationDef ruleApplication, string message, Signature author, Signature committer)
        {
            if (ruleApplication == null) throw new ArgumentNullException(nameof(ruleApplication));
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (author == null) throw new ArgumentNullException(nameof(author));
            if (committer == null) throw new ArgumentNullException(nameof(committer));

            var headTarget = _repository.Refs.Head.ResolveToDirectReference();
            IEnumerable<Commit> parents = Enumerable.Empty<Commit>();

            if (headTarget != null)
            {
                parents = new[] { _repository.Lookup<Commit>(_repository.Refs.Head.TargetIdentifier) };
            }

            var commit = _repository.ObjectDatabase.CreateCommit(author, committer, message, ruleApplication, parents, true);

            if (headTarget != null)
            {
                _repository.Refs.UpdateTarget(_repository.Refs.Head.TargetIdentifier, commit.Sha);
            }
            else
            {
                _repository.Refs.Add(_repository.Refs.Head.TargetIdentifier, commit.Sha);
            }

            return commit;
        }

        /// <summary>
        /// Create a new branch from the current branch.
        /// </summary>
        /// <param name="branchName">The branch name for the new branch.</param>
        /// <returns>The created branch.</returns>
        public Branch CreateBranch(string branchName)
        {
            if (branchName == null) throw new ArgumentNullException(nameof(branchName));
            if (string.IsNullOrWhiteSpace(branchName)) throw new ArgumentException("Specified branch name cannot be null or whitespace.", nameof(branchName));

            var targetRef = _repository.Refs[$"refs/heads/{branchName}"];

            if (targetRef != null)
            {
                throw new ArgumentException("Specified branch already exists; cannot create a branch.", nameof(branchName));
            }

            //_repository.Refs.UpdateTarget(_repository.Refs.Head, $"refs/heads/develop");
            // TODO: Add test to create branch when no commits exist
            return _repository.CreateBranch(branchName);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing,
        /// or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _repository.Dispose();
        }

        /// <summary>
        /// Fetch all of the latest changes from a remote InRule git repository.
        /// </summary>
        /// <param name="options">The parameters that control the fetch behavior.</param>
        public void Fetch(FetchOptions options)
        {
            Fetch("origin", options);
        }

        /// <summary>
        /// Fetch all of the latest changes from a remote InRule git repository.
        /// </summary>
        /// <param name="remote">The name or URI for the remote repository.</param>
        /// <param name="options">The parameters that control the fetch behavior.></param>
        public void Fetch(string remote, FetchOptions options)
        {
            if (remote == null) throw new ArgumentNullException(nameof(remote));
            if (string.IsNullOrWhiteSpace(remote)) throw new ArgumentException("Specified remote cannot be null or whitespace.", nameof(remote));

            var remoteObj = _repository.Network.Remotes[remote];

            if (remoteObj == null)
            {
                throw new NotImplementedException();
            }

            var fetchRefSpecs = remoteObj.FetchRefSpecs.Select(x => x.Specification);

            options = options ?? new FetchOptions();

            _repository.Network.Fetch(remoteObj.Url, fetchRefSpecs, new LibGit2Sharp.FetchOptions
            {
                CertificateCheck = options.CertificateCheck,
                CredentialsProvider = options.CredentialsProvider
            });
        }

        /// <summary>
        /// Get a rule application from the current branch.
        /// </summary>
        /// <param name="ruleApplicationName">The case-insensitive rule application name.</param>
        /// <returns>A rule application found in the current branch that has the specified
        /// name; null otherwise.</returns>
        public RuleApplicationDef GetRuleApplication(string ruleApplicationName)
        {
            if (ruleApplicationName == null) throw new ArgumentNullException(nameof(ruleApplicationName));
            if (string.IsNullOrWhiteSpace(ruleApplicationName)) throw new ArgumentException("Specified rule application name cannot be null or whitespace.", nameof(ruleApplicationName));

            var headTarget = _repository.Refs.Head.ResolveToDirectReference();

            if (headTarget == null)
            {
                return null;
            }

            var commit = _repository.Lookup<Commit>(headTarget.TargetIdentifier);

            return commit.GetRuleApplication(ruleApplicationName);
        }

        /// <summary>
        /// Remove an existing branch.
        /// </summary>
        /// <param name="branchName">The branch name to remove.</param>
        public void RemoveBranch(string branchName)
        {
            if (branchName == null) throw new ArgumentNullException(nameof(branchName));
            if (string.IsNullOrWhiteSpace(branchName)) throw new ArgumentException("Specified branch name cannot be null or whitespace.", nameof(branchName));

            var targetRef = _repository.Refs[$"refs/heads/{branchName}"];

            if (targetRef == null)
            {
                throw new ArgumentException("Specified branch name does not exist; cannot remove branch.", nameof(branchName));
            }

            if (targetRef == _repository.Refs.Head.ResolveToDirectReference())
            {
                throw new ArgumentException("Specified branch is the current branch; checkout a different branch before attempting to remove this one.", nameof(branchName));
            }

            _repository.Branches.Remove(branchName);
        }

        /// <summary>
        /// Clone a remote InRule git repository to a new local repository.
        /// </summary>
        /// <param name="sourceUrl">The URI for the remote repository.</param>
        /// <param name="destinationPath">The local destination path to clone into.</param>
        /// <param name="options">The parameters that control the clone behavior.</param>
        /// <returns>The path to the created repository.</returns>
        public static string Clone(string sourceUrl, string destinationPath, CloneOptions options)
        {
            if (sourceUrl == null) throw new ArgumentNullException(nameof(sourceUrl));
            if (string.IsNullOrWhiteSpace(sourceUrl)) throw new ArgumentException("Specified source URL cannot be null or whitespace.", nameof(sourceUrl));

            if (destinationPath == null) throw new ArgumentNullException(nameof(destinationPath));
            if (string.IsNullOrWhiteSpace(destinationPath)) throw new ArgumentException("Specified destination path cannot be null or whitespace.", nameof(destinationPath));

            if (File.Exists(destinationPath))
            {
                throw new ArgumentException(
                    "Specified destination path is a file and cannot be used as an InRule Git repository.",
                    nameof(destinationPath));
            }

            if (IsValid(destinationPath))
            {
                throw new ArgumentException(
                    "Specified destination path already contains an existing git repository; cannot be used to clone an InRule Git repository.",
                    nameof(destinationPath));
            }

            if (Directory.Exists(destinationPath) && Directory.EnumerateFileSystemEntries(destinationPath).Any())
            {
                throw new ArgumentException(
                    "Specified destination path is not empty; cannot be used to clone an InRule Git repository.",
                    nameof(destinationPath));
            }

            // TODO: What if the sourceUrl is not a valid Git repo or a valid InRule Git repo?

            options = options ?? new CloneOptions();

            return LibGit2Sharp.Repository.Clone(sourceUrl, destinationPath, new LibGit2Sharp.CloneOptions
            {
                CertificateCheck = options.CertificateCheck,
                CredentialsProvider = options.CredentialsProvider,
                IsBare = true
            });
        }

        /// <summary>
        /// Initialize a new InRule git repository at the specified path.
        /// </summary>
        /// <param name="path">The local path to initialize the repository.</param>
        /// <returns>The path to the created repository.</returns>
        public static string Init(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Specified path cannot be null or whitespace.", nameof(path));

            if (File.Exists(path))
            {
                throw new ArgumentException(
                    "Specified path is a file and cannot be used as an InRule Git repository.",
                    nameof(path));
            }

            if (IsValid(path))
            {
                throw new ArgumentException(
                    "Specified path already contains an existing git repository; cannot initialize a new Git repository.",
                    nameof(path));
            }

            if (Directory.Exists(path) && Directory.EnumerateFileSystemEntries(path).Any())
            {
                throw new ArgumentException(
                    "Specified path is not empty; cannot initialize a new Git repository.",
                    nameof(path));
            }

            return LibGit2Sharp.Repository.Init(path, true);
        }

        /// <summary>
        /// Check if the specified path is a valid InRule git repository.
        /// </summary>
        /// <param name="path">The local path used for verification.</param>
        /// <returns>True if the path is a valid InRule git repository; false otherwise.</returns>
        public static bool IsValid(string path)
        {
            return LibGit2Sharp.Repository.IsValid(path);
        }

        /// <summary>
        /// Initialize a new instance of InRuleGitRepository for an existing
        /// repository at the specified path.
        /// </summary>
        /// <param name="path">The path to the existing InRule git repository.</param>
        /// <returns>A new instance of InRuleGitRepository for the specified path.</returns>
        public static InRuleGitRepository Open(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Specified path cannot be null or whitespace.", nameof(path));

            if (!IsValid(path))
            {
                throw new ArgumentException(
                    "Specified path is not a valid Git repository.",
                    nameof(path));
            }

            var repository = new LibGit2Sharp.Repository(path);

            return new InRuleGitRepository(repository);
        }
    }
}
