namespace Sknet.InRuleGitStorage.Tests.InRuleGitRepositoryTests;

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
        Assert.Throws<ArgumentNullException>(() => repository.Checkout(null!));
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

    /*[Fact]
    public void WithNonExistingBranch_ShouldThrowException()
    {
        // Arrange
        var repository = new InRuleGitRepository(_fixture.Repository);

        // Act/Assert
        Assert.Throws<ArgumentException>(() => repository.Checkout("unknown-branch"));
    }*/

    [Fact]
    public void WithNonMainBranch_ShouldUpdateHead()
    {
        // Arrange
        var repository = new InRuleGitRepository(_fixture.Repository);

        var identity = new Identity("Peter Quill", "starlord@gotg.org");
        var signature = new Signature(identity, DateTimeOffset.UtcNow);
        _fixture.Repository.Commit("", signature, signature);

        _fixture.Repository.CreateBranch("develop");
        Assert.Equal(_fixture.Repository.Config.GetDefaultBranch(), _fixture.Repository.Head.FriendlyName);

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

    /*[Fact]
    public void Playground()
    {
        using (var localFixture = new GitRepositoryFixture())
        using (var remoteFixture = new GitRepositoryFixture())
        {
            var localRepository = new InRuleGitRepository(localFixture.Repository);
            var remoteRepository = new InRuleGitRepository(remoteFixture.Repository);

            //localRepository.Config.Set("user.name", "Neil Armstrong");
            //localRepository.Config.Set("user.name", "Neil Armstrong");

            localRepository.Remotes.Add("origin", remoteFixture.Repository.Info.Path);

            var ruleAppDef = new RuleApplicationDef("MyRuleApplication");
            remoteRepository.Commit(ruleAppDef, "My git to remote repo");

            remoteRepository.CreateBranch("branchName");
            ruleAppDef = new RuleApplicationDef("AnotherRuleApplication");
            remoteRepository.Commit(ruleAppDef, "Another my git to remote repo");

            localRepository.Fetch(new FetchOptions());

            localRepository.CreateTrackedBranch("branchName", "origin");
            localRepository.Checkout("branchName");

            var ruleAppDef2 = localRepository.GetRuleApplication("AnotherRuleApplication");
        }
    }

    [Fact]
    public void Playground2()
    {
        using (var localFixture = new GitRepositoryFixture())
        {
            var localRepository = new InRuleGitRepository(localFixture.Repository);

            //localRepository.CreateBranch("myBranchName");
            localRepository.Checkout("myBranchName");

            var ruleAppDef = new RuleApplicationDef("MyRuleApplication");
            localRepository.Commit(ruleAppDef, "Another my git to remote repo");
        }
    }*/
}
