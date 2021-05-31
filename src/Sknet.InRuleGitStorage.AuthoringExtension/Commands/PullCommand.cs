using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Common.Utilities;
using InRule.Repository;
using InRule.Repository.Client;
using System;
using System.Windows;

namespace Sknet.InRuleGitStorage.AuthoringExtension.Commands
{
    public class PullCommand : VisualCommandBase
    {
        public PullCommand()
            : base("[Git].[Actions].PullCommand",
                "Pull",
                ImageFactory.GetImageThisAssembly("Images\\GitPull16.png"),
                ImageFactory.GetImageThisAssembly("Images\\GitPull32.png"),
                isEnabled: false)
        {
            // Label = "Pull 39↓";
            Subscribe(
               Subscription.RuleApplicationOpened,
               Subscription.RuleApplicationChanged,
               Subscription.RuleApplicationClosed);
        }

        protected override void WhenRuleApplicationOpened(object sender, EventArgs e)
        {
            IsEnabled = RuleApplicationService.PersistenceInfo.IsGitRepository();
        }

        protected override void WhenRuleApplicationDefChanged(object sender, EventArgs<RuleApplicationDef> e)
        {
            if (!RuleApplicationService.PersistenceInfo.IsGitRepository())
            {
                IsEnabled = false;
                return;
            }

            var persistenceInfo = (GitPersistenceInfo)RuleApplicationService.PersistenceInfo;
            var workingDirectory = persistenceInfo.RepositoryOption.WorkingDirectory;

            using var repo = new LibGit2Sharp.Repository(workingDirectory);
            var currentBranch = repo.Branches[repo.Head.FriendlyName];
            var behindBy = currentBranch.TrackingDetails.BehindBy;

            Label = behindBy != null && behindBy > 0 ? $"Pull {behindBy}↓" : "Pull";
        }

        protected override void WhenRuleApplicationClosed(object sender, EventArgs e)
        {
            IsEnabled = false;
        }

        public override void Execute()
        {
            // HACK
            RuleApplicationService.CheckOut(RuleAppCheckOutMode.CompleteRuleApplication);


            var persistenceInfo = (GitPersistenceInfo)RuleApplicationService.PersistenceInfo;
            var workingDirectory = persistenceInfo.RepositoryOption.WorkingDirectory;

            using var repo = new LibGit2Sharp.Repository(workingDirectory);
            var currentBranch = repo.Branches[repo.Head.FriendlyName];
            var behindBy = currentBranch.TrackingDetails.BehindBy;

            Application.Current.Dispatcher.Invoke(() =>
            {
                Label = behindBy != null && behindBy > 0 ? $"Pull {behindBy}↓" : "Pull";
            });
        }


    }
}
