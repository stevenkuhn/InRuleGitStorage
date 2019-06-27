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

            /*var notes = new StringBuilder();
            foreach (var treeEntry in commit.Tree)
            {
                //_repository.Lookup<Commit>(ObjectId.TryParse())
                var logEntry = _repository.Commits.QueryBy(treeEntry.Name, new CommitFilter() { FirstParentOnly = true }).First();
                notes.AppendFormat("{0},{1}\n", treeEntry.Name, logEntry.Commit.Committer.When.ToUniversalTime());
            }
            notes.Remove(notes.Length - 1, 1);
            _repository.Notes.Add(commit.Id, notes.ToString(), author, committer, "inrule/git");*/

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

        public void Fetch()
        {
            throw new NotImplementedException();

            /*
            var remote = _repository.Network.Remotes["origin"];
            var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
            var fetchOptions = new FetchOptions
            {
                CredentialsProvider = null
            };
            var logMessage = "";

            Commands.Fetch((LibGit2Sharp.Repository)_repository, remote.Name, refSpecs, fetchOptions, logMessage);*/
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

        public RuleApplicationGitInfo[] GetRuleApplications()
        {
            throw new NotImplementedException();
        }

        /*/// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RuleApplicationSummary> GetRuleApplicationSummaries()
        {
            var headTarget = _repository.Refs.Head.ResolveToDirectReference();

            if (headTarget == null)
            {
                throw new NotImplementedException();
            }

            var commit = _repository.Lookup<Commit>(headTarget.TargetIdentifier);
            var note = commit.Notes.FirstOrDefault(n => n.Namespace == "inrule/git");

            if (note == null)
            {
                return Enumerable.Empty<RuleApplicationSummary>();
            }

            var reader = new StringReader(note.Message);

            var summaries = new List<RuleApplicationSummary>();

            while (true)
            {
                var line = reader.ReadLine();
                if (line == null)
                {
                    break;
                }

                var values = line.Split(',');
                var name = values[0];
                var lastModifiedOn = DateTimeOffset.Parse(values[1]);

                summaries.Add(new RuleApplicationSummary { Name = name, LastModifiedOn = lastModifiedOn });
            }

            return summaries;
        }*/

        public MergeResult Merge(string branchName, Signature merger)
        {
            throw new NotImplementedException();

            /*if (branchName == null) throw new ArgumentNullException(nameof(branchName));
            if (string.IsNullOrWhiteSpace(branchName)) throw new ArgumentException("Specified branch name cannot be null or whitespace.", nameof(branchName));
            if (merger == null) throw new ArgumentNullException(nameof(merger));

            var targetRef = _repository.Refs[$"refs/heads/{branchName}"];

            if (targetRef == null)
            {
                throw new ArgumentException("Specified branch name does not exist; cannot merge.", nameof(branchName));
            }

            var currentBranchCommit = _repository.Lookup<Commit>(_repository.Refs.Head.TargetIdentifier);
            var branchCommit = _repository.Lookup<Commit>(targetRef.TargetIdentifier);

            if (_repository.ObjectDatabase.CanMergeWithoutConflict(currentBranchCommit, branchCommit))
            {
                var mergeTreeResult = _repository.ObjectDatabase.MergeCommits(currentBranchCommit, branchCommit, new MergeTreeOptions
                {
                    
                });

                var mergeCommit = _repository.ObjectDatabase.CreateCommit(
                    merger, 
                    merger, 
                    $"Merge branch '{targetRef.CanonicalName.Replace("refs/heads/", "")}' into {_repository.Refs.Head.TargetIdentifier.Replace("refs/heads/", "")}", 
                    mergeTreeResult.Tree,
                    new [] { currentBranchCommit, branchCommit }, true);

                _repository.Refs.UpdateTarget(_repository.Refs.Head.TargetIdentifier, mergeCommit.Sha);

                return mergeTreeResult;
            }

            throw new NotImplementedException("Merge conflicts have been detected and support for merge conflicts are not supported yet; cannot merge.");*/
        }

        public MergeResult Pull(Signature merger)
        {
            throw new NotImplementedException();

            /*
            var pullOptions = new PullOptions
            {
                FetchOptions = new FetchOptions
                {
                    CredentialsProvider = null
                },
                MergeOptions = new MergeOptions
                {
                    
                }
            };

            var mergeResult = Commands.Pull((LibGit2Sharp.Repository)_repository, merger, pullOptions);

            return mergeResult;

            /* Credential information to fetch
    LibGit2Sharp.PullOptions options = new LibGit2Sharp.PullOptions();
    options.FetchOptions = new FetchOptions();
    options.FetchOptions.CredentialsProvider = new CredentialsHandler(
        (url, usernameFromUrl, types) =>
            new UsernamePasswordCredentials()
            {
                Username = USERNAME,
                Password = PASSWORD
            });

    // User information to create a merge commit
    var signature = new LibGit2Sharp.Signature(
        new Identity("MERGE_USER_NAME", "MERGE_USER_EMAIL"), DateTimeOffset.Now);

    // Pull
    Commands.Pull(repo, signature, options);*/
        }

        public void Push(string branchName)
        {
            throw new NotImplementedException();

            /*	Remote remote = repo.Network.Remotes["origin"];
	var options = new PushOptions();
	options.CredentialsProvider = (_url, _user, _cred) => 
		new UsernamePasswordCredentials { Username = "USERNAME", Password = "PASSWORD" };
	repo.Network.Push(remote, @"refs/heads/master", options);*/
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
        /// 
        /// </summary>
        /// <param name="sourceUrl"></param>
        /// <param name="destinationPath"></param>
        /// <returns></returns>
        public static string Clone(string sourceUrl, string destinationPath)
        {
            return Clone(sourceUrl, destinationPath, null);
        }

        /// <summary>
        /// Clone a remote InRule git repository to a new local repository.
        /// </summary>
        /// <param name="sourceUrl">The URI for the remote repository.</param>
        /// <param name="destinationPath">The local destination path to clone into.</param>
        /// <param name="credentialsProvider"></param>
        /// <returns>The path to the created repository.</returns>
        public static string Clone(string sourceUrl, string destinationPath, CredentialsHandler credentialsProvider)
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

            return LibGit2Sharp.Repository.Clone(sourceUrl, destinationPath, new CloneOptions
            {
                Checkout = false,
                CredentialsProvider = credentialsProvider,
                IsBare = true,
            });

            /*(url, usernameFromUrl, types) => new UsernamePasswordCredentials
            {
                Username = username,
                Password = password
            }*/
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
