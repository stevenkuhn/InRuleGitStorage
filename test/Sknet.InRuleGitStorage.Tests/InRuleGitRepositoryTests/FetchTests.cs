namespace Sknet.InRuleGitStorage.Tests.InRuleGitRepositoryTests;

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
    public void WithNullRemote_ShouldThrowException()
    {
        // Arrange
        var repository = new InRuleGitRepository(_localFixture.Repository);

        // Act/Assert
        Assert.Throws<ArgumentNullException>(() => repository.Fetch(null!, new FetchOptions()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void WithWhiteSpaceRemote_ShouldThrowException(string remote)
    {
        // Arrange
        var repository = new InRuleGitRepository(_localFixture.Repository);

        // Act/Assert
        Assert.Throws<ArgumentException>(() => repository.Fetch(remote, new FetchOptions()));
    }

    [Fact]
    public void WithNoRemotes_ShouldThrowException()
    {
        // Arrange
        var repository = new InRuleGitRepository(_localFixture.Repository);

        // Act/Assert
        Assert.Throws<ArgumentException>(() => repository.Fetch("unknown-remote", new FetchOptions()));
    }

    [Fact]
    public void WithNonExistingRemote_ShouldThrowException()
    {
        // Arrange
        var remoteRepository = new InRuleGitRepository(_remoteFixture.Repository);
        var localRepository = new InRuleGitRepository(_localFixture.Repository);

        localRepository.Remotes.Add("origin", _remoteFixture.Repository.Info.Path);

        // Act/Assert
        Assert.Throws<ArgumentException>(() => localRepository.Fetch("unknown-remote", new FetchOptions()));
    }

    [Fact]
    public void WithExistingRemoteWithNoCommits_ShouldDoSomething()
    {
        // Arrange
        var localRepository = new InRuleGitRepository(_localFixture.Repository);

        localRepository.Remotes.Add("origin", _remoteFixture.Repository.Info.Path);

        var ruleAppDef = new RuleApplicationDef("InvoiceSample");

        var message = "This is a test commit";
        var identity = new Identity("Peter Quill", "starlord@gotg.org");
        var signature = new Signature(identity, DateTimeOffset.UtcNow);
        localRepository.Commit(ruleAppDef, message, signature, signature);

        // Act
        localRepository.Fetch("origin", new FetchOptions());

        // Assert
        var originRef = _localFixture.Repository.Refs[$"refs/remotes/origin/{_localFixture.Repository.Config.GetDefaultBranch()}"];
        Assert.Null(originRef);
    }

    [Fact]
    public void WithExistingRemoteWithOneCommit_ShouldMatchRefs()
    {
        // Arrange
        var remoteRepository = new InRuleGitRepository(_remoteFixture.Repository);
        var localRepository = new InRuleGitRepository(_localFixture.Repository);

        localRepository.Remotes.Add("origin", _remoteFixture.Repository.Info.Path);

        var ruleAppDef = new RuleApplicationDef("InvoiceSample");

        var localMessage = "This is a test commit to local repository";
        var identity = new Identity("Peter Quill", "starlord@gotg.org");
        var signature = new Signature(identity, DateTimeOffset.UtcNow);
        localRepository.Commit(ruleAppDef, localMessage, signature, signature);

        var remoteMessage = "This is a test commit to remote repository";
        remoteRepository.Commit(ruleAppDef, remoteMessage, signature, signature);

        // Act
        localRepository.Fetch("origin", new FetchOptions());

        // Assert
        var originRef = _localFixture.Repository.Refs[$"refs/remotes/origin/{_localFixture.Repository.Config.GetDefaultBranch()}"];
        Assert.NotNull(originRef);
        Assert.Equal(_remoteFixture.Repository.Refs[$"refs/heads/{_remoteFixture.Repository.Config.GetDefaultBranch()}"].TargetIdentifier, originRef.TargetIdentifier);
    }

    [Fact]
    public void WithExistingRemoteWithTwoBranches_ShouldMatchRefs()
    {
        // Arrange
        var remoteRepository = new InRuleGitRepository(_remoteFixture.Repository);
        var localRepository = new InRuleGitRepository(_localFixture.Repository);

        localRepository.Remotes.Add("origin", _remoteFixture.Repository.Info.Path);

        var ruleAppDef = new RuleApplicationDef("InvoiceSample");

        var localMessage = "This is a test commit to local repository";
        var identity = new Identity("Peter Quill", "starlord@gotg.org");
        var signature = new Signature(identity, DateTimeOffset.UtcNow);
        localRepository.Commit(ruleAppDef, localMessage, signature, signature);

        var remoteMessage = "This is a test commit to remote repository";
        remoteRepository.Commit(ruleAppDef, remoteMessage, signature, signature);

        remoteRepository.CreateBranch("branch1");
        remoteRepository.Checkout("branch1");
        remoteRepository.Commit(ruleAppDef, remoteMessage, signature, signature);

        // Act
        localRepository.Fetch("origin", new FetchOptions());

        // Assert
        var originMainRef = _localFixture.Repository.Refs[$"refs/remotes/origin/{_localFixture.Repository.Config.GetDefaultBranch()}"];
        var originBranch1Ref = _localFixture.Repository.Refs["refs/remotes/origin/branch1"];
        Assert.NotNull(originMainRef);
        Assert.NotNull(originBranch1Ref);

        Assert.Equal(_remoteFixture.Repository.Refs[$"refs/heads/{_localFixture.Repository.Config.GetDefaultBranch()}"].TargetIdentifier, originMainRef.TargetIdentifier);
        Assert.Equal(_remoteFixture.Repository.Refs["refs/heads/branch1"].TargetIdentifier, originBranch1Ref.TargetIdentifier);
    }
}
