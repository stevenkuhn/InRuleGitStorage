using InRule.Repository;
using LibGit2Sharp;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Xunit;
using InRuleContrib.Repository.Storage.Git.Tests.Fixtures;

namespace InRuleContrib.Repository.Storage.Git.Tests.InRuleGitRepositoryTests
{
    public class CommitTests : IDisposable
    {
        private readonly GitRepositoryFixture _fixture;

        public CommitTests()
        {
            _fixture = new GitRepositoryFixture(isBare: false);
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }

        [Fact]
        public void WithNullRuleApplication_ShouldThrowException()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);
            var message = "This is a test commit";

            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);

            // Act/Assert
            Assert.Throws<ArgumentNullException>(() => repository.Commit(null, message, signature, signature));
        }

        [Fact]
        public void WithNullMessage_ShouldThrowException()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);
            var ruleApplication = new RuleApplicationDef();

            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);

            // Act/Assert
            Assert.Throws<ArgumentNullException>(() => repository.Commit(ruleApplication, null, signature, signature));
        }

        [Fact]
        public void WithNullAuthor_ShouldThrowException()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);
            var ruleApplication = new RuleApplicationDef();
            var message = "This is a test commit";

            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);

            // Act/Assert
            Assert.Throws<ArgumentNullException>(() => repository.Commit(ruleApplication, message, null, signature));
        }

        [Fact]
        public void WithNullCommitter_ShouldThrowException()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);
            var ruleApplication = new RuleApplicationDef();
            var message = "This is a test commit";

            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);

            // Act/Assert
            Assert.Throws<ArgumentNullException>(() => repository.Commit(ruleApplication, message, signature, null));
        }

        [Fact]
        public void WithNoOtherCommits_ShouldAddCommit()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);
            var ruleApplication = new RuleApplicationDef();
            var message = "This is a test commit";

            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);

            Assert.False(_fixture.Repository.Commits.Any());

            // Act
            repository.Commit(ruleApplication, message, signature, signature);

            // Assert
            Assert.Single(_fixture.Repository.Commits);
            Assert.NotNull(_fixture.Repository.Refs.Head.ResolveToDirectReference());
        }

        [Fact]
        public void WithOneCommit_ShouldAddCommitWithParent()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);
            var ruleApplication = new RuleApplicationDef();
            var message = "This is a test commit";

            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);

            var parentCommit = _fixture.Repository.Commit("", signature, signature);
            Assert.Single(_fixture.Repository.Commits);

            // Act
            repository.Commit(ruleApplication, message, signature, signature);

            // Assert
            Assert.Equal(2, _fixture.Repository.Commits.Count());
            var commit = _fixture.Repository.Lookup<Commit>(_fixture.Repository.Refs.Head.TargetIdentifier);
            Assert.Equal(parentCommit, commit.Parents.ElementAt(0));
        }

        [Fact(Skip = "Not sure what to do here...")]
        public void WithParentThatIsYoungerThanCommit_ShouldDoWhat()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);
            var ruleApplication = new RuleApplicationDef();

            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var parentSignature = new Signature(identity, DateTimeOffset.UtcNow);

            var parentCommit = _fixture.Repository.Commit("This is the parent commit", parentSignature, parentSignature);
            Assert.Single(_fixture.Repository.Commits);

            var commitSignature = new Signature(identity, DateTimeOffset.UtcNow.AddYears(-1));

            // Act 
            repository.Commit(ruleApplication, "This is the test commit", commitSignature, commitSignature);

             // Assert
            Assert.Equal(2, _fixture.Repository.Commits.Count());
            var commit = _fixture.Repository.Lookup<Commit>(_fixture.Repository.Refs.Head.TargetIdentifier);
            Assert.Equal(parentCommit, commit.Parents.ElementAt(0));
        }
    }
}
