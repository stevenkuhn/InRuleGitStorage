using InRule.Repository;
using LibGit2Sharp;
using Sknet.InRuleGitStorage.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sknet.InRuleGitStorage
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
        /// <returns>The generated commit containing the specified rule application and any existing rule applications.</returns>
        public Commit Commit(RuleApplicationDef ruleApplication, string message)
        {
            var signature = Config.BuildSignature(DateTimeOffset.Now);

            return Commit(ruleApplication, message, signature, signature);
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
        /// Provides access to the configuration settings for this repository.
        /// </summary>
        public Configuration Config { get { return _repository.Config; } }

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
                throw new ArgumentException("Specified remote name does not exist; cannot fetch.", nameof(remote));
            }

            var fetchRefSpecs = remoteObj.FetchRefSpecs.Select(x => x.Specification);

            options = options ?? new FetchOptions();

            _repository.Network.Fetch(remoteObj.Name, fetchRefSpecs, new LibGit2Sharp.FetchOptions
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
        /// Get a collection of references to available rule application from the current branch.
        /// </summary>
        /// <returns>A collection of rule application references (not the actual rule applications) from the current branch.</returns>
        public RuleApplicationGitInfo[] GetRuleApplications()
        {
            var headTarget = _repository.Refs.Head.ResolveToDirectReference();

            if (headTarget == null)
            {
                return new RuleApplicationGitInfo[0];
            }

            var ruleApplications = new List<RuleApplicationGitInfo>();
            var commit = _repository.Lookup<Commit>(headTarget.TargetIdentifier);

            foreach (var treeEntry in commit.Tree)
            {
                if (treeEntry?.Target == null || !(treeEntry.Target is Tree))
                {
                    continue;
                }

                //var ruleAppDef = serializer.Deserialize(treeEntry);

                var tree = (Tree)treeEntry.Target;
                var blob = (Blob)tree[$"{treeEntry.Name}.xml"].Target;

                var xml = blob.GetContentText();
                var type = typeof(RuleApplicationDef);
                var def = (RuleApplicationDef)RuleRepositoryDefBase.LoadFromXml(xml, type);

                var logEntry = _repository.Commits.QueryBy(treeEntry.Name, new CommitFilter() { FirstParentOnly = true }).First();

                var info = new RuleApplicationGitInfo(def, logEntry.Commit);
                ruleApplications.Add(info);
            }

            return ruleApplications.ToArray();
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

        /// <summary>
        /// Perform a merge of the current branch and the specified branch, and
        /// create a commit if there are no conflicts.
        /// </summary>
        /// <param name="branchName">The branch name to merge with the current branch.</param>
        /// <param name="options">The parameters that control the merge behavior.</param>
        /// <returns>The result of a merge of two trees and any conflicts.</returns>
        public MergeTreeResult Merge(string branchName, MergeOptions options)
        {
            var signature = Config.BuildSignature(DateTimeOffset.Now);

            return Merge(branchName, signature, options);
        }

        /// <summary>
        /// Perform a merge of the current branch and the specified branch, and
        /// create a commit if there are no conflicts.
        /// </summary>
        /// <param name="branchName">The branch name to merge with the current branch.</param>
        /// <param name="merger">The signature to use for the merge.</param>
        /// <param name="options">The parameters that control the merge behavior.</param>
        /// <returns>The result of a merge of two trees and any conflicts.</returns>
        public MergeTreeResult Merge(string branchName, Signature merger, MergeOptions options)
        {
            if (branchName == null) throw new ArgumentNullException(nameof(branchName));
            if (string.IsNullOrWhiteSpace(branchName)) throw new ArgumentException("Specified branch name cannot be null or whitespace.", nameof(branchName));
            if (merger == null) throw new ArgumentNullException(nameof(merger));

            var targetRef = _repository.Refs[$"refs/heads/{branchName}"];

            if (targetRef == null)
            {
                throw new ArgumentException("Specified branch name does not exist; cannot merge.", nameof(branchName));
            }

            return MergeFromReference(targetRef, merger, options);
        }

        private MergeTreeResult MergeFromReference(Reference reference, Signature merger, MergeOptions options)
        {
            if (reference == null) throw new ArgumentNullException(nameof(reference));
            if (merger == null) throw new ArgumentNullException(nameof(merger));

            // TODO: what if head doesn't exist?

            var baseCommit = _repository.Lookup<Commit>(_repository.Refs.Head.TargetIdentifier);
            var headCommit = _repository.Lookup<Commit>(reference.TargetIdentifier);

            if (!_repository.ObjectDatabase.CanMergeWithoutConflict(one: baseCommit, another: headCommit))
            {
                throw new NotImplementedException("Merge conflicts have been detected and support for merge conflicts are not supported yet; cannot merge.");
            }

            // TODO: Set MergeTreeOptions
            var mergeTreeResult = _repository.ObjectDatabase.MergeCommits(
                ours: baseCommit,
                theirs: headCommit,
                options: new MergeTreeOptions
                {
                });

            // TODO: fix message when pulling from remote
            var mergeCommit = _repository.ObjectDatabase.CreateCommit(
                author: merger,
                committer: merger,
                message: $"Merge branch '{reference.CanonicalName.Replace("refs/heads/", "")}' into {_repository.Refs.Head.TargetIdentifier.Replace("refs/heads/", "")}",
                tree: mergeTreeResult.Tree,
                parents: new[] { baseCommit, headCommit },
                prettifyMessage: true);

            _repository.Refs.UpdateTarget(_repository.Refs.Head.TargetIdentifier, mergeCommit.Sha);

            return mergeTreeResult;
        }

        /// <summary>
        /// Fetch all of the changes from a remote InRule git repository and
        /// merge into the current branch.
        /// </summary>
        /// <param name="options">The parameters that control the fetch and merge behavior.</param>
        /// <returns>The result of a merge of two trees and any conflicts.</returns>
        public MergeTreeResult Pull(PullOptions options)
        {
            var signature = Config.BuildSignature(DateTimeOffset.Now);

            return Pull(signature, options);
        }

        /// <summary>
        /// Fetch all of the changes from a remote InRule git repository and
        /// merge into the current branch.
        /// </summary>
        /// <param name="merger">The signature to use for the merge.</param>
        /// <param name="options">The parameters that control the fetch and merge behavior.</param>
        /// <returns>The result of a merge of two trees and any conflicts.</returns>
        public MergeTreeResult Pull(Signature merger, PullOptions options)
        {
            return Pull("origin", merger, options);
        }

        /// <summary>
        /// Fetch all of the changes from a remote InRule git repository and
        /// merge into the current branch.
        /// </summary>
        /// <param name="remote">The name or URI for the remote repository.</param>
        /// <param name="options">The parameters that control the fetch and merge behavior.</param>
        /// <returns>The result of a merge of two trees and any conflicts.</returns>
        public MergeTreeResult Pull(string remote, PullOptions options)
        {
            var signature = Config.BuildSignature(DateTimeOffset.Now);

            return Pull(remote, signature, options);
        }

        /// <summary>
        /// Fetch all of the changes from a remote InRule git repository and
        /// merge into the current branch.
        /// </summary>
        /// <param name="remote">The name or URI for the remote repository.</param>
        /// <param name="merger">The signature to use for the merge.</param>
        /// <param name="options">The parameters that control the fetch and merge behavior.</param>
        /// <returns>The result of a merge of two trees and any conflicts.</returns>
        public MergeTreeResult Pull(string remote, Signature merger, PullOptions options)
        {
            // TODO: Add PullOptions

            if (remote == null) throw new ArgumentNullException(nameof(remote));
            if (string.IsNullOrWhiteSpace(remote)) throw new ArgumentException("Specified remote cannot be null or whitespace.", nameof(remote));
            if (merger == null) throw new ArgumentNullException(nameof(merger));

            // TODO: Set Fetch options
            Fetch(remote, new FetchOptions());

            // TODO: What if this doesn't resolve?
            var referenceName = _repository.Refs.Head.ResolveToDirectReference().CanonicalName.Replace("refs/heads/", $"refs/remotes/{remote}/");
            var reference = _repository.Refs[referenceName];

            return MergeFromReference(reference, merger, new MergeOptions());
        }

        /// <summary>
        /// Push the current branch to a remote InRule git repository.
        /// </summary>
        /// <param name="options">The parameters that control the push behavior.</param>
        public void Push(PushOptions options)
        {
            Push("origin", options);
        }

        /// <summary>
        /// Push the current branch to a remote InRule git repository.
        /// </summary>
        /// <param name="remote">The name or URI for the remote repository.</param>
        /// <param name="options">The parameters that control the push behavior.</param>
        public void Push(string remote, PushOptions options)
        {
            if (remote == null) throw new ArgumentNullException(nameof(remote));
            if (string.IsNullOrWhiteSpace(remote)) throw new ArgumentException("Specified remote cannot be null or whitespace.", nameof(remote));

            var remoteObj = _repository.Network.Remotes[remote];

            if (remoteObj == null)
            {
                throw new ArgumentException("Specified remote name does not exist; cannot fetch.", nameof(remote));
            }

            options = options ?? new PushOptions();

            _repository.Network.Push(
                remote: remoteObj,
                pushRefSpec: _repository.Refs.Head.ResolveToDirectReference().CanonicalName,
                pushOptions: new LibGit2Sharp.PushOptions
                {
                });
        }

        /// <summary>
        /// Lookup and manage remotes in the repository.
        /// </summary>
        public RemoteCollection Remotes => _repository.Network.Remotes;

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
        /// <returns>The path to the created repository.</returns>
        public static string Clone(string sourceUrl, string destinationPath)
        {
            return Clone(sourceUrl, destinationPath, null);
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

            var result = LibGit2Sharp.Repository.Clone(sourceUrl, destinationPath, new LibGit2Sharp.CloneOptions
            {
                CertificateCheck = options.CertificateCheck,
                CredentialsProvider = options.CredentialsProvider,
                IsBare = true
            });

            /*(url, usernameFromUrl, types) => new UsernamePasswordCredentials
            {
                Username = username,
                Password = password
            }*/

            SetFolderIcon(destinationPath);

            return result;
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

            var result = LibGit2Sharp.Repository.Init(path, true);

            SetFolderIcon(path);

            return result;
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

        /// <summary>
        /// Initialize a new instance of InRuleGitRepository for an existing
        /// repository at the specified path, or create a new InRule git
        /// repository if it doesn't exist.
        /// </summary>
        /// <param name="path">The path to an existing InRule git repository or path to intialize a new repository.</param>
        /// <returns>A new instance of InRuleGitRepository for the specified path.</returns>
        public static InRuleGitRepository OpenOrInit(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Specified path cannot be null or whitespace.", nameof(path));

            if (!IsValid(path))
            {
                Init(path);
            }

            return Open(path);
        }

        private static void SetFolderIcon(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Specified path cannot be null or whitespace.", nameof(path));

            File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.System);

            var logoPath = Path.Combine(path, "logo.ico");
            using (Stream input = typeof(InRuleGitRepository).Assembly.GetManifestResourceStream("Sknet.InRuleGitStorage.logo.ico"))
            using (Stream output = File.Create(logoPath))
            {
                input.Seek(0, SeekOrigin.Begin);
                input.CopyTo(output);
            }

            var desktopIniPath = Path.Combine(path, "desktop.ini");
            File.WriteAllText(desktopIniPath, @"[.ShellClassInfo]
IconResource = .\logo.ico,0");

            File.SetAttributes(desktopIniPath, File.GetAttributes(desktopIniPath) | FileAttributes.Hidden | FileAttributes.System);
            File.SetAttributes(logoPath, File.GetAttributes(logoPath) | FileAttributes.Hidden | FileAttributes.System);
        }
    }
}
