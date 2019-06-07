using InRuleContrib.Repository.Storage.Git.Tests.Fixtures;
using LibGit2Sharp;
using System;
using Xunit;

namespace InRuleContrib.Repository.Storage.Git.Tests.InRuleGitRepositoryTests
{
    public class RemoveBranchTests : IDisposable
    {
        private readonly GitRepositoryFixture _fixture;

        public RemoveBranchTests()
        {
            _fixture = new GitRepositoryFixture();
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }

        [Fact]
        public void WithNullBranchName_ShouldThrowException()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);

            // Act/Assert
            Assert.Throws<ArgumentNullException>(() => repository.RemoveBranch(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void WithWhiteSpaceBranchName_ShouldThrowException(string path)
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);

            // Act/Assert
            Assert.Throws<ArgumentException>(() => repository.RemoveBranch(path));
        }

        [Fact]
        public void WithNonExistingBranch_ShouldThrowException()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);

            // Act/Assert
            Assert.Throws<ArgumentException>(() => repository.RemoveBranch("unknown-branch"));
        }

        [Fact]
        public void WithCurrentHeadBranch_ShouldThrowException()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);

            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);
            _fixture.Repository.Commit("", signature, signature);

            var branch = _fixture.Repository.CreateBranch("develop");
            _fixture.Repository.Refs.UpdateTarget(_fixture.Repository.Refs.Head, branch.Reference);

            // Act/Assert
            Assert.Throws<ArgumentException>(() => repository.RemoveBranch("develop"));
        }

        [Fact]
        public void WithExistingNotHeadBranch_ShouldRemoveBranch()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);

            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);
            _fixture.Repository.Commit("", signature, signature);

            var branch = _fixture.Repository.CreateBranch("develop");

            // Act
            repository.RemoveBranch("develop");

            // Assert
            var targetRef = _fixture.Repository.Refs[$"refs/heads/develop"];
            Assert.Null(targetRef);
        }
    }
}
