using InRule.Repository;
using InRuleContrib.Repository.Storage.Git.Tests.Fixtures;
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace InRuleContrib.Repository.Storage.Git.Tests.InRuleGitRepositoryTests
{
    public class SerializationTests : IDisposable
    {
        private readonly GitRepositoryFixture _fixture;

        public SerializationTests()
        {
            _fixture = new GitRepositoryFixture();
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }

        [Theory]
        [InlineData("Chicago Food Tax Generator.ruleappx")]
        [InlineData("InvoiceSample.ruleappx")]
        [InlineData("UDF Examples.ruleappx")]
        [InlineData("Vocabulary Examples.ruleappx")]
        public void CommitAndGet_ShouldMatchXmlOfRuleApplication(string ruleAppFileName)
        {
            // Arrange
            var repository = new InRuleGitRepository(_fixture.Repository);
            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var signature = new Signature(identity, DateTimeOffset.UtcNow);

            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"InRuleContrib.Repository.Storage.Git.Tests.RuleApps.{ruleAppFileName}";
            var stream = assembly.GetManifestResourceStream(resourceName);

            var ruleApp = RuleApplicationDef.Load(stream);
            var ruleAppName = ruleApp.Name;

            stream.Dispose();

            // Act
            repository.Commit(ruleApp, "This is a test commit", signature, signature);
            var result = repository.GetRuleApplication(ruleAppName);

            //var test = repository.GetRuleApplicationSummaries();

            // Assert
            var expectedXml = RuleRepositoryDefBase.GetXml(ruleApp);
            var resultXml = RuleRepositoryDefBase.GetXml(result);

            Assert.Equal(expectedXml, resultXml);
        }
    }
}
