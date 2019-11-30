using InRule.Repository;
using LibGit2Sharp;
using Sknet.InRuleGitStorage.Tests.Fixtures;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Xunit;

namespace Sknet.InRuleGitStorage.Tests.InRuleGitRepositoryTests
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
            var resourceName = $"Sknet.InRuleGitStorage.Tests.RuleApps.{ruleAppFileName}";
            var stream = assembly.GetManifestResourceStream(resourceName);

            var ruleApp = RuleApplicationDef.Load(stream);
            var ruleAppName = ruleApp.Name;

            stream.Dispose();

            // Act
            repository.Commit(ruleApp, "This is a test commit", signature, signature);
            var result = repository.GetRuleApplication(ruleAppName);

            // Assert
            var expectedXml = RuleRepositoryDefBase.GetXml(ruleApp);
            var resultXml = RuleRepositoryDefBase.GetXml(result);

            Assert.Equal(expectedXml, resultXml);
        }
    }
}
