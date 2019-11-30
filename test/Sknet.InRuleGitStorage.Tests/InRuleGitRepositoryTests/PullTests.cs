using InRule.Repository;
using LibGit2Sharp;
using Sknet.InRuleGitStorage.Tests.Fixtures;
using System;
using Xunit;

namespace Sknet.InRuleGitStorage.Git.Tests.InRuleGitRepositoryTests
{
    public class PullTests : IDisposable
    {
        private readonly GitRepositoryFixture _localFixture;
        private readonly GitRepositoryFixture _remoteFixture;

        public PullTests()
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
            var identity = new Identity("Peter Quill", "starlord@gotg.org");
            var remoteRepository = new InRuleGitRepository(_remoteFixture.Repository);
            var localRepository = new InRuleGitRepository(_localFixture.Repository);

            localRepository.Remotes.Add("origin", _remoteFixture.Repository.Info.Path);

            var ruleAppDef1 = new RuleApplicationDef("RuleApplication1");
            localRepository.Commit(
                ruleAppDef1,
                "Commit of ruleApp1",
                new Signature(identity, DateTimeOffset.UtcNow),
                new Signature(identity, DateTimeOffset.UtcNow));

            var ruleAppDef2 = new RuleApplicationDef("RuleApplication2");
            remoteRepository.Commit(
                ruleAppDef2,
                "Commit of ruleApp2",
                new Signature(identity, DateTimeOffset.UtcNow),
                new Signature(identity, DateTimeOffset.UtcNow));

            var mergeResult = localRepository.Pull(
                new Signature(identity, DateTimeOffset.UtcNow),
                new PullOptions());

            Assert.NotNull(mergeResult);
            Assert.Equal(MergeTreeStatus.Succeeded, mergeResult.Status);

            localRepository.Checkout("master");
            var result1 = localRepository.GetRuleApplication("RuleApplication1");
            var result2 = localRepository.GetRuleApplication("RuleApplication2");

            Assert.NotNull(result1);
            Assert.Equal(ruleAppDef1.Name, result1.Name);
            Assert.Equal(ruleAppDef1.Guid, result1.Guid);

            Assert.NotNull(result2);
            Assert.Equal(ruleAppDef2.Name, result2.Name);
            Assert.Equal(ruleAppDef2.Guid, result2.Guid);
        }
    }
}
