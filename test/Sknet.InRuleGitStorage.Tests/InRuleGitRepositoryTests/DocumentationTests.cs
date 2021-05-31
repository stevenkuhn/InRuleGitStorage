using InRule.Repository;
using Sknet.InRuleGitStorage.Tests.Fixtures;
using System;
using System.Reflection;
using Xunit;

namespace Sknet.InRuleGitStorage.Tests.InRuleGitRepositoryTests
{
    public class DocumentationTests : IDisposable
    {
        private readonly TemporaryDirectoryFixture _fixture;

        public DocumentationTests()
        {
            _fixture = new TemporaryDirectoryFixture();
        }

        public void Dispose()
        {
            _fixture.Dispose();
        }

        [Fact]
        public void Introduction_BasicExample_Test()
        {
            // Create a new repository in a local directory
            InRuleGitRepository.Init(_fixture.DirectoryPath);

            // Get a new instance of your local InRule Git repository
            using (var repo = InRuleGitRepository.Open(_fixture.DirectoryPath))
            {
                repo.Config.Set("user.name", "Starlord");
                repo.Config.Set("user.email", "starlord@gotg.com");

                // Create a new rule application and commit it to the "master" branch
                var ruleApp = new RuleApplicationDef("QuickstartSample");
                repo.Commit(ruleApp, "Add quickstart sample rule application");

                // Get the rule application from the Git repository
                var result = repo.GetRuleApplication("QuickstartSample");

                // Assert
                Assert.Equal(ruleApp.Guid, result.Guid);
            }
        }

        [Fact]
        public void Introduction_RemoteRepositoryExample_Test()
        {
            // Arrage
            using (var remoteFixture = new GitRepositoryFixture())
            {
                using (var repo = new LibGit2Sharp.Repository(remoteFixture.Repository.Info.Path))
                {
                    repo.Config.Set("inrule.enabled", true);
                }

                using (var repo = InRuleGitRepository.Open(remoteFixture.Repository.Info.Path))
                {
                    repo.Config.Set("user.name", "Starlord");
                    repo.Config.Set("user.email", "starlord@gotg.com");

                    var ruleApp = new RuleApplicationDef("BlankRuleApp");
                    repo.Commit(ruleApp, "Add blank rule application");

                    repo.CreateBranch("v0.2.0");
                    repo.Checkout("v0.2.0");

                    var assembly = Assembly.GetExecutingAssembly();
                    var resourceName = $"Sknet.InRuleGitStorage.Tests.RuleApps.InvoiceSample.ruleappx";
                    var stream = assembly.GetManifestResourceStream(resourceName);
                    ruleApp = RuleApplicationDef.Load(stream);
                    repo.Commit(ruleApp, "Add invoice sample rule application");

                    resourceName = $"Sknet.InRuleGitStorage.Tests.RuleApps.Chicago Food Tax Generator.ruleappx";
                    stream = assembly.GetManifestResourceStream(resourceName);
                    ruleApp = RuleApplicationDef.Load(stream);
                    repo.Commit(ruleApp, "Add Chicago Food Tax Generator rule application");

                    repo.Checkout("master");
                }

                // === Test ===

                InRuleGitRepository.Clone(
                    sourceUrl: remoteFixture.Repository.Info.Path,
                    destinationPath: _fixture.DirectoryPath);

                // Get a new instance of your local InRule Git repository
                using (var repo = InRuleGitRepository.Open(_fixture.DirectoryPath))
                {
                    repo.Config.Set("user.name", "Starlord");
                    repo.Config.Set("user.email", "starlord@gotg.com");

                    // Create a local branch that is tracked to the remote "v0.2.0" branch
                    repo.CreateBranch("v0.2.0", "origin");

                    // Switch the current branch to the newly created tracked branch
                    repo.Checkout("v0.2.0");

                    // Create a local branch from the "v0.2.0" branch
                    repo.CreateBranch("invoice-date-field");

                    // Switch the current branch to the newly created local branch
                    repo.Checkout("invoice-date-field");

                    // Get the InvoiceSample rule application from the repository, add an invoice date
                    // field, and commit that change to the current branch
                    var ruleApp = repo.GetRuleApplication("InvoiceSample");
                    ruleApp.Entities["Invoice"].Fields.Add(new FieldDef("Date", DataType.DateTime));
                    repo.Commit(ruleApp, "Add invoice date field");

                    // Switch back to the previous branch that does not have the field change
                    repo.Checkout("v0.2.0");

                    // Merge the invoice date field change into the current branch
                    repo.Merge("invoice-date-field");

                    // Delete the original branch containing the invoice date field change since the
                    // change now exists in the "v0.2.0" branch
                    repo.RemoveBranch("invoice-date-field");

                    repo.Push();
                }

                // === ==== ===
                using (var repo = InRuleGitRepository.Open(remoteFixture.Repository.Info.Path))
                {
                    repo.Config.Set("user.name", "Starlord");
                    repo.Config.Set("user.email", "starlord@gotg.com");

                    repo.Checkout("v0.2.0");
                    var ruleApp = repo.GetRuleApplication("InvoiceSample");
                    repo.Checkout("master");

                    var field = ruleApp.Entities["Invoice"].Fields["Date"];

                    Assert.NotNull(field);
                    Assert.Equal(DataType.DateTime, field.DataType);

                    repo.Checkout("v0.2.0");
                    repo.RemoveRuleApplication("InvoiceSample", "Remove rule application");
                    ruleApp = repo.GetRuleApplication("InvoiceSample");
                    var ruleApps = repo.GetRuleApplications();
                    repo.Checkout("master");

                    Assert.Null(ruleApp);
                    Assert.Equal(2, ruleApps.Length);
                }
            }
        }
    }
}
