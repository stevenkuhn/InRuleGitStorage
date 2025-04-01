using System.Text.RegularExpressions;

namespace Sknet.InRuleGitStorage.Tests.InRuleGitRepositoryTests;

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

        Assert.NotNull(stream);

        var ruleApp = RuleApplicationDef.Load(stream);
        var ruleAppName = ruleApp.Name;

        stream!.Dispose();

        // Act
        repository.Commit(ruleApp, "This is a test commit", signature, signature);
        var result = repository.GetRuleApplication(ruleAppName);

        // Assert
        var expectedXml = RuleRepositoryDefBase.GetXml(ruleApp);
        var resultXml = RuleRepositoryDefBase.GetXml(result);

        // HACK: Replace the Guid in <DefaultSubRulesRoot /> with an Empty guid because it changes on every call to GetXml()
        string pattern = @"(<DefaultSubRulesRoot\s+[^>]*Guid="")[^""]*("")";
        expectedXml = Regex.Replace(expectedXml, pattern, $"$1{Guid.Empty}$2");
        resultXml = Regex.Replace(resultXml, pattern, $"$1{Guid.Empty}$2");

        Assert.Equal(expectedXml, resultXml);
    }
}
