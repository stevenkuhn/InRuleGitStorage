using InRule.Repository;
using InRuleContrib.Repository.Storage.Git.Tests.Fixtures;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace InRuleContrib.Repository.Storage.Git.Tests.InRuleGitRepositoryTests
{
    public class GetRuleApplicationTests : IDisposable
    {
        private readonly GitRepositoryFixture _fixture;

        public GetRuleApplicationTests()
        {
            _fixture = new GitRepositoryFixture();
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }

        [Fact]
        public void WithNullRuleApplicationName_ShouldThrowException()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);

            // Act/Assert
            Assert.Throws<ArgumentNullException>(() => repository.GetRuleApplication(null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("  ")]
        public void WithWhiteSpaceRuleApplicatinoName_ShouldThrowException(string ruleApplicationName)
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);

            // Act/Assert
            Assert.Throws<ArgumentException>(() => repository.GetRuleApplication(ruleApplicationName));
        }

        [Fact]
        public void WithNoCommits_ShouldReturnNull()
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);
            
            // Act
            var result = repository.GetRuleApplication("myRuleApp");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void WithNoRuleApplications_ShouldReturnNull()
        {
             // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);
           
            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);
            _fixture.Repository.Commit("", signature, signature);
            
            // Act
            var result = repository.GetRuleApplication("myRuleApp");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void WithUnknownRuleApplication_ShouldReturnNull()
        {
             // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);
           
            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);
            repository.Commit(new RuleApplicationDef("AnotherRuleApp"), "This is an example commit", signature, signature);
            
            // Act
            var result = repository.GetRuleApplication("myRuleApp");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void WithExistingRuleApplication_ShouldReturnRuleApplication()
        {
             // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);
           
            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);
            repository.Commit(new RuleApplicationDef("myRuleApp"), "This is an example commit", signature, signature);
            
            // Act
            var result = repository.GetRuleApplication("myRuleApp");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("myRuleApp", result.Name);
        }
    }
}
