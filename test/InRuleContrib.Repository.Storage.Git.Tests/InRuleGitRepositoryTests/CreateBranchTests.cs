using InRuleContrib.Repository.Storage.Git.Tests.Fixtures;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace InRuleContrib.Repository.Storage.Git.Tests.InRuleGitRepositoryTests
{
    public class CreateBranchTests : IDisposable
    {
        private readonly GitRepositoryFixture _fixture;

        public CreateBranchTests()
        {
            _fixture = new GitRepositoryFixture(isBare: false);
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
            Assert.Throws<ArgumentNullException>(() => repository.CreateBranch(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void WithWhiteSpaceBranchName_ShouldThrowException(string path)
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);

            // Act/Assert
            Assert.Throws<ArgumentException>(() => repository.CreateBranch(path));
        }

        [Fact]
        public void WithExistingBranch_ShouldThrowException()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);

            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);
            _fixture.Repository.Commit("", signature, signature);

            _fixture.Repository.CreateBranch("develop");

            // Act/Assert
            Assert.Throws<ArgumentException>(() => repository.CreateBranch("develop"));
        }

        [Fact]
        public void WithNonExistingBranch_ShouldCreateBranchAtHead()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);

            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);
            _fixture.Repository.Commit("", signature, signature);

            // Act
            repository.CreateBranch("develop");

            // Assert
            Assert.Equal(
                _fixture.Repository.Refs["refs/heads/master"].TargetIdentifier,
                _fixture.Repository.Refs["refs/heads/develop"].TargetIdentifier);
        }

        /*[Fact]
        public void WithNonExistingBranchAndNoCommits_ShouldCreateBranchAtHead()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);

            /var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);
            _fixture.Repository.Commit("", signature, signature);/

            // Act
            repository.CreateBranch("develop");

            // Assert
            Assert.Equal(
                _fixture.Repository.Refs["refs/heads/master"].TargetIdentifier,
                _fixture.Repository.Refs["refs/heads/develop"].TargetIdentifier);
        }*/
    }
}
