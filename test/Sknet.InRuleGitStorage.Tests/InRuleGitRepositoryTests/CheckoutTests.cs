using InRule.Repository;
using LibGit2Sharp;
using Sknet.InRuleGitStorage.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Xunit;

namespace Sknet.InRuleGitStorage.Tests.InRuleGitRepositoryTests
{
    public class CheckoutTests : IDisposable
    {
        private readonly GitRepositoryFixture _fixture;

        public CheckoutTests()
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
            Assert.Throws<ArgumentNullException>(() => repository.Checkout(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void WithWhiteSpaceBranchName_ShouldThrowException(string path)
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);

            // Act/Assert
            Assert.Throws<ArgumentException>(() => repository.Checkout(path));
        }

        [Fact]
        public void WithNonExistingBranch_ShouldThrowException()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);

            // Act/Assert
            Assert.Throws<ArgumentException>(() => repository.Checkout("unknown-branch"));
        }

        [Fact]
        public void WithNonMasterBranch_ShouldUpdateHead()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);

            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);
            _fixture.Repository.Commit("", signature, signature);

            _fixture.Repository.CreateBranch("develop");
            Assert.Equal("master", _fixture.Repository.Head.FriendlyName);

            // Act
            repository.Checkout("develop");

            // Assert
            Assert.Equal("develop", _fixture.Repository.Head.FriendlyName);
        }

        [Fact]
        public void WithSameBranch_ShouldUpdateHead()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);

            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);
            _fixture.Repository.Commit("", signature, signature);

            _fixture.Repository.CreateBranch("develop");
            repository.Checkout("develop");
            Assert.Equal("develop", _fixture.Repository.Head.FriendlyName);

            // Act
            repository.Checkout("develop");

            // Assert
            Assert.Equal("develop", _fixture.Repository.Head.FriendlyName);
        }
    }
}
