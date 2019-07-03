using InRule.Repository;
using InRuleContrib.Repository.Storage.Git.Tests.Fixtures;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using Xunit;

namespace InRuleContrib.Repository.Storage.Git.Tests.InRuleGitRepositoryTests
{
    public class FetchTests : IDisposable
    {
        private readonly GitRepositoryFixture _localFixture;
        private readonly GitRepositoryFixture _remoteFixture;

        public FetchTests()
        {
            _localFixture = new GitRepositoryFixture();
            _remoteFixture = new GitRepositoryFixture();
        }

        public void Dispose()
        {
            _localFixture.Dispose();
            _remoteFixture.Dispose();
        }

        [Fact]
        public void Playground()
        {
            var ruleAppDef = new RuleApplicationDef("InvoiceSample");

            var remoteRepository = new InRuleGitRepository(_remoteFixture.Repository);
            var message = "This is a test commit";

            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);

            remoteRepository.Commit(ruleAppDef, message, signature, signature);

            var localRepository = new InRuleGitRepository(_localFixture.Repository);
            localRepository.Remotes.Add("origin", _remoteFixture.Repository.Info.Path);

            localRepository.Fetch(new FetchOptions());

            localRepository.Checkout("origin/master");

            var test = localRepository.GetRuleApplication("InvoiceSample");

            Assert.NotNull(test);

            /*var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            Assert.True(Directory.Exists(path));

            try
            {
                var ruleAppDef = new RuleApplicationDef("InvoiceSample");

                var repository = new InRuleGitRepository(_fixture.Repository);
                var message = "This is a test commit";

                var identity = new Identity("Peter Quill", "starlord@gotg.org");
                var signature = new Signature(identity, DateTimeOffset.UtcNow);

                repository.Commit(ruleAppDef, message, signature, signature);

                //InRuleGitRepository.Clone(_fixture.Repository.Info.Path, path, new CloneOptions());
                //var localRepo = 

                repository.Dispose();
                //Thread.Sleep(5000);
            }
            finally
            {
                Directory.Delete(path, true);
            }*/
        }
    }

    public class CheckoutTests : IDisposable
    {
        private readonly GitRepositoryFixture _fixture;

        public CheckoutTests()
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
