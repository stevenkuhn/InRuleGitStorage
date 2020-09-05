using InRule.Repository;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

            if (!_repository.Refs.Any())
            {
                _repository.Refs.UpdateTarget("HEAD", $"refs/heads/{branchName}");
                return;
            }

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

            var defHashToTreeShaLookup = new Dictionary<string, string>();

            var parentsList = new List<Commit>(parents);
            if (parentsList.Count > 1) throw new NotImplementedException();

            var allRuleAppsTreeDefinition = new TreeDefinition();
            if (parentsList.Count == 1)
            {
                var parentTree = parentsList[0].Tree;

                foreach (var treeEntry in parentTree)
                {
                    if (string.Equals(treeEntry.Name, ruleApplication.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    allRuleAppsTreeDefinition.Add(treeEntry.Name, treeEntry);
                }

                var lookupNote = _repository.Notes["inrule/git/refs", parentsList[0].Id];
                if (lookupNote != null)
                {
                    using (var reader = new StringReader(lookupNote.Message))
                    {
                        while (true)
                        {
                            var line = reader.ReadLine();
                            if (string.IsNullOrWhiteSpace(line))
                            {
                                break;
                            }

                            var parts = line.Split(':');
                            defHashToTreeShaLookup[parts[0]] = parts[1];
                        }
                    }
                }
            }

            ruleApplication = (RuleApplicationDef)ruleApplication.CopyWithSameGuids();

            IInRuleGitSerializer serializer = new InRuleGitSerializer(_repository, defHashToTreeShaLookup);
            var ruleAppTree = serializer.Serialize(ruleApplication);

            allRuleAppsTreeDefinition.Add(ruleApplication.Name, ruleAppTree);
            var allRuleAppsTree = _repository.ObjectDatabase.CreateTree(allRuleAppsTreeDefinition);

            var commit = _repository.ObjectDatabase.CreateCommit(author, committer, message, allRuleAppsTree, parentsList, true, null);

            if (headTarget != null)
            {
                _repository.Refs.UpdateTarget(_repository.Refs.Head.TargetIdentifier, commit.Sha);
            }
            else
            {
                _repository.Refs.Add(_repository.Refs.Head.TargetIdentifier, commit.Sha);
            }

            var sb = new StringBuilder();
            foreach (var keyValuePair in defHashToTreeShaLookup)
            {
                sb.AppendLine($"{keyValuePair.Key}:{keyValuePair.Value}");
            }

            _repository.Notes.Add(commit.Id, sb.ToString(), author, committer, "inrule/git/refs");

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

            var branch = _repository.Branches[branchName];

            if (branch != null)
            {
                throw new ArgumentException("Specified branch already exists; cannot create a branch.", nameof(branchName));
            }

            // TODO: Add test to create branch when no commits exist
            return _repository.CreateBranch(branchName);
        }

        /// <summary>
        /// Create a new tracked branch from the remote branch of the same name.
        /// </summary>
        /// <param name="branchName">The branch name for the new branch.</param>
        /// <param name="remote">The name for the remote repository.</param>
        /// <returns>The created branch.</returns>
        public Branch CreateBranch(string branchName, string remote)
        {
            if (branchName == null) throw new ArgumentNullException(nameof(branchName));
            if (string.IsNullOrWhiteSpace(branchName)) throw new ArgumentException("Specified branch name cannot be null or whitespace.", nameof(branchName));
            if (remote == null) throw new ArgumentNullException(nameof(remote));
            if (string.IsNullOrWhiteSpace(remote)) throw new ArgumentException("Specified remote cannot be null or whitespace.", nameof(remote));

            var branch = _repository.Branches[branchName];

            if (branch != null)
            {
                throw new ArgumentException("Specified branch already exists; cannot create a branch.", nameof(branchName));
            }

            var remoteBranch = _repository.Branches[$"{remote}/{branchName}"];

            if (!remoteBranch.IsRemote)
            {
                throw new NotImplementedException();
            }

            branch = _repository.CreateBranch(branchName, remoteBranch.Tip);

            _repository.Branches.Update(branch, b => b.TrackedBranch = remoteBranch.CanonicalName);

            return branch;
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
                CredentialsProvider = options.CredentialsProvider,
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

            var ruleAppTreeEntry = commit.Tree[ruleApplicationName];

            if (ruleAppTreeEntry?.Target == null || !(ruleAppTreeEntry.Target is Tree))
            {
                return null;
            }

            IInRuleGitSerializer serializer = new InRuleGitSerializer(_repository);
            return serializer.Deserialize(ruleAppTreeEntry);
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

                var tree = (Tree)treeEntry.Target;
                var blob = (Blob)tree[$"{treeEntry.Name}.xml"].Target;

                var xml = blob.GetContentText();
                var type = typeof(RuleApplicationDef);
                var def = (RuleApplicationDef)RuleRepositoryDefBase.LoadFromXml(xml, type);

                var logEntry = _repository.Commits.QueryBy(treeEntry.Path, new CommitFilter() { SortBy = CommitSortStrategies.Time, FirstParentOnly = false }).First();

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

            var defHashToTreeShaLookup = new Dictionary<string, string>();

            var lookupNote = _repository.Notes["inrule/git/refs", baseCommit.Id];
            if (lookupNote != null)
            {
                using (var reader = new StringReader(lookupNote.Message))
                {
                    while (true)
                    {
                        var line = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            break;
                        }

                        var parts = line.Split(':');
                        defHashToTreeShaLookup[parts[0]] = parts[1];
                    }
                }
            }

            lookupNote = _repository.Notes["inrule/git/refs", headCommit.Id];
            if (lookupNote != null)
            {
                using (var reader = new StringReader(lookupNote.Message))
                {
                    while (true)
                    {
                        var line = reader.ReadLine();
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            break;
                        }

                        var parts = line.Split(':');
                        defHashToTreeShaLookup[parts[0]] = parts[1];
                    }
                }
            }

            var sb = new StringBuilder();
            foreach (var keyValuePair in defHashToTreeShaLookup)
            {
                sb.AppendLine($"{keyValuePair.Key}:{keyValuePair.Value}");
            }

            _repository.Notes.Add(mergeCommit.Id, sb.ToString(), merger, merger, "inrule/git/refs");

            return mergeTreeResult;
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

            options = options ?? new PullOptions();

            var fetchOptions = options.FetchOptions ?? new FetchOptions();
            var mergeOptions = options.MergeOptions ?? new MergeOptions();

            Fetch(remote, fetchOptions);

            // TODO: What if this doesn't resolve?
            var referenceName = _repository.Refs.Head.ResolveToDirectReference().CanonicalName.Replace("refs/heads/", $"refs/remotes/{remote}/");
            var reference = _repository.Refs[referenceName];

            return MergeFromReference(reference, merger, mergeOptions);
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
                    CertificateCheck = options.CertificateCheck,
                    CredentialsProvider = options.CredentialsProvider
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
        /// Create a commit that removes the specified rule application from the current branch.
        /// </summary>
        /// <param name="ruleApplicationName">The case-insensitive rule application name.</param>
        /// <param name="message">The description of why a change was made to the repository.</param>
        /// <param name="author">The signature of who made the change.</param>
        /// <param name="committer">The signature of who added the change to the repository.</param>
        public Commit RemoveRuleApplication(string ruleApplicationName, string message, Signature author, Signature committer)
        {
            if (ruleApplicationName == null) throw new ArgumentNullException(nameof(ruleApplicationName));
            if (string.IsNullOrWhiteSpace(ruleApplicationName)) throw new ArgumentException("Specified rule application name cannot be null or whitespace.", nameof(ruleApplicationName));
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (author == null) throw new ArgumentNullException(nameof(author));
            if (committer == null) throw new ArgumentNullException(nameof(committer));

            var headTarget = _repository.Refs.Head.ResolveToDirectReference();

            if (headTarget == null)
            {
                return null;
            }

            var commit = _repository.Lookup<Commit>(headTarget.TargetIdentifier);

            var ruleAppTreeEntry = commit.Tree[ruleApplicationName];

            if (ruleAppTreeEntry?.Target == null || !(ruleAppTreeEntry.Target is Tree))
            {
                return null;
            }

            var parents = new[] { _repository.Lookup<Commit>(_repository.Refs.Head.TargetIdentifier) };

            var allRuleAppsTreeDefinition = new TreeDefinition();

            var parentTree = parents[0].Tree;

            foreach (var treeEntry in parentTree)
            {
                if (string.Equals(treeEntry.Name, ruleAppTreeEntry.Name, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                allRuleAppsTreeDefinition.Add(treeEntry.Name, treeEntry);
            }

            var allRuleAppsTree = _repository.ObjectDatabase.CreateTree(allRuleAppsTreeDefinition);
            commit = _repository.ObjectDatabase.CreateCommit(author, committer, message, allRuleAppsTree, parents, true, null);

            _repository.Refs.UpdateTarget(_repository.Refs.Head.TargetIdentifier, commit.Sha);

            return commit;
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

            if (Repository.IsValid(destinationPath))
            {
                throw new ArgumentException(
                    "Specified destination path already contains an existing Git repository; cannot be used to clone an InRule Git repository.",
                    nameof(destinationPath));
            }

            if (IsValid(destinationPath))
            {
                throw new ArgumentException(
                    "Specified destination path already contains an existing InRule Git repository; cannot be used to clone an InRule Git repository.",
                    nameof(destinationPath));
            }

            if (Directory.Exists(destinationPath) && Directory.EnumerateFileSystemEntries(destinationPath).Any())
            {
                throw new ArgumentException(
                    "Specified destination path is not empty; cannot be used to clone an InRule Git repository.",
                    nameof(destinationPath));
            }

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            destinationPath = Path.GetFullPath(destinationPath);

            SetFolderIcon(destinationPath);

            if (IsWindowsOSPlatform())
            {
                destinationPath = Path.Combine(destinationPath, "tmp");
            }

            // TODO: What if the sourceUrl is not a valid Git repo or a valid InRule Git repo?

            options = options ?? new CloneOptions();

            var result = LibGit2Sharp.Repository.Clone(sourceUrl, destinationPath, new LibGit2Sharp.CloneOptions
            {
                CertificateCheck = options.CertificateCheck,
                CredentialsProvider = options.CredentialsProvider,
                IsBare = true
            });

            if (IsWindowsOSPlatform())
            {
                var directoryInfo = new DirectoryInfo(destinationPath);
                var files = directoryInfo.GetFiles();
                var directories = directoryInfo.GetDirectories();

                foreach (var directory in directories)
                {
                    directory.MoveTo(Path.Combine(directoryInfo.Parent.FullName, directory.Name));
                }

                foreach (var file in files)
                {
                    file.MoveTo(Path.Combine(directoryInfo.Parent.FullName, file.Name));
                }

                directoryInfo.Delete();
                destinationPath = directoryInfo.Parent.FullName;
            }

            using (var repo = new Repository(destinationPath))
            {
                repo.Config.Set("inrule.enabled", true, ConfigurationLevel.Local);

                repo.Network.Remotes.Update("origin", r =>
                {
                    r.FetchRefSpecs.Add("+refs/notes/inrule/*:refs/notes/inrule/*");
                    r.PushRefSpecs.Add("+refs/notes/inrule/*:refs/notes/inrule/*");
                });

                var remoteObj = repo.Network.Remotes["origin"];
                var fetchRefSpecs = remoteObj.FetchRefSpecs.Select(x => x.Specification);

                repo.Network.Fetch(remoteObj.Name, fetchRefSpecs, new LibGit2Sharp.FetchOptions
                {
                    CertificateCheck = options.CertificateCheck,
                    CredentialsProvider = options.CredentialsProvider,
                });
            }

            return destinationPath;
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

            if (Repository.IsValid(path))
            {
                throw new ArgumentException(
                    "Specified path already contains an existing Git repository; cannot initialize a new InRule Git repository.",
                    nameof(path));
            }

            if (IsValid(path))
            {
                throw new ArgumentException(
                    "Specified path already contains an existing InRule Git repository; cannot initialize a new InRule Git repository.",
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

            using var repo = new Repository(path);
            repo.Config.Set("inrule.enabled", true, ConfigurationLevel.Local);

            return result;
        }

        /// <summary>
        /// Check if the specified path is a valid InRule git repository.
        /// </summary>
        /// <param name="path">The local path used for verification.</param>
        /// <returns>True if the path is a valid InRule git repository; false otherwise.</returns>
        public static bool IsValid(string path)
        {
            if (!LibGit2Sharp.Repository.IsValid(path))
            {
                return false;
            }

            using var repo = new Repository(path);
            var config = repo.Config.Get<bool>("inrule.enabled", ConfigurationLevel.Local);

            return config != null && config.Value;
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
                    "Specified path is not a valid InRule Git repository.",
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
            if (!IsWindowsOSPlatform())
            {
                return;
            }

            if (path == null) throw new ArgumentNullException(nameof(path));
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Specified path cannot be null or whitespace.", nameof(path));

            File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.ReadOnly);

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

        private static bool IsWindowsOSPlatform()
        {
#if NETSTANDARD
            return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#elif NETFRAMEWORK
            return Environment.OSVersion.Platform == PlatformID.Win32NT; 
#else
            throw new NotSupportedException("IsWindowsOSPlayform() is only supported with NETSTANDARD and NETFRAMEWORK directives.");
#endif
        }
    }
}
