using InRule.Repository;
using InRuleContrib.Repository.Storage.Git.Tests.Fixtures;
using LibGit2Sharp;
using System;
using Xunit;

namespace InRuleContrib.Repository.Storage.Git.Tests.InRuleGitRepositoryTests
{
    public class PushTests : IDisposable
    {
        private readonly GitRepositoryFixture _localFixture;
        private readonly GitRepositoryFixture _remoteFixture;

        public PushTests()
        {
            _localFixture = new GitRepositoryFixture(isBare: true);
            _remoteFixture = new GitRepositoryFixture(isBare: true);
        }

        public void Dispose()
        {
            _localFixture.Dispose();
            _remoteFixture.Dispose();
        }

        [Fact]
        public void Playground()
        {
            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var remoteRepository = new InRuleGitRepository(_remoteFixture.Repository);
            var localRepository = new InRuleGitRepository(_localFixture.Repository);

            localRepository.Remotes.Add("origin", _remoteFixture.Repository.Info.Path);
            localRepository.Commit(
                new RuleApplicationDef("LocalRuleApplication"),
                "Commit of local ruleApp",
                new Signature(identity, DateTimeOffset.UtcNow),
                new Signature(identity, DateTimeOffset.UtcNow));

            var ruleAppDef = new RuleApplicationDef("RemoteRuleApplication");
            remoteRepository.Commit(
                ruleAppDef,
                "Commit of remote ruleApp",
                new Signature(identity, DateTimeOffset.UtcNow),
                new Signature(identity, DateTimeOffset.UtcNow));

            localRepository.Pull(new Signature(identity, DateTimeOffset.UtcNow), new PullOptions());
            ruleAppDef = localRepository.GetRuleApplication("RemoteRuleApplication");
            ruleAppDef.Entities.Add(new EntityDef("Entity1"));
            localRepository.Commit(
                ruleAppDef,
                "Commit of remote ruleApp to local repo",
                new Signature(identity, DateTimeOffset.UtcNow),
                new Signature(identity, DateTimeOffset.UtcNow));

            localRepository.Push(new PushOptions());

            var remoteRuleAppDef = remoteRepository.GetRuleApplication("RemoteRuleApplication");
            Assert.NotNull(remoteRuleAppDef);
            Assert.NotEmpty(remoteRuleAppDef.Entities);
            Assert.Equal("Entity1", remoteRuleAppDef.Entities[0].Name);
        }
    }
}
