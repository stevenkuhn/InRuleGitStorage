using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Authoring.Services;
using InRule.Authoring.Windows;
using InRule.Common.Utilities;
using InRule.Repository;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace Sknet.InRuleGitStorage.AuthoringExtension.Commands
{
    public class PushCommand : VisualCommandBase
    {
        public PushCommand()
            : base("[Git].[Actions].PushCommand",
                "Push",
                ImageFactory.GetImageThisAssembly("Images\\GitPush16.png"),
                ImageFactory.GetImageThisAssembly("Images\\GitPush32.png"),
                isEnabled: false)
        {
            // Label = "Push 6↑";
            Subscribe(
                   Subscription.RuleApplicationOpened,
                   Subscription.RuleApplicationChanged,
                   Subscription.RuleApplicationClosed);
        }

        protected override void WhenRuleApplicationOpened(object sender, EventArgs e)
        {
            IsEnabled = RuleApplicationService.PersistenceInfo.IsGitRepository();

            if (RuleApplicationService.PersistenceInfo.IsGitRepository())
            {
                ((GitRuleApplicationServiceImpl)RuleApplicationService.Implementation).RuleApplicationCommitted += PushCommand_RuleApplicationCommitted;
            }
        }

        private void PushCommand_RuleApplicationCommitted(object sender, EventArgs e)
        {
            UpdateLabel();
        }

        protected override void WhenRuleApplicationDefChanged(object sender, EventArgs<RuleApplicationDef> e)
        {
            if (!RuleApplicationService.PersistenceInfo.IsGitRepository())
            {
                IsEnabled = false;
                Label = "Push";
                return;
            }

            UpdateLabel();
        }

        private void UpdateLabel()
        {
            var persistenceInfo = (GitPersistenceInfo)RuleApplicationService.PersistenceInfo;
            var workingDirectory = persistenceInfo.RepositoryOption.WorkingDirectory;

            using var repo = new LibGit2Sharp.Repository(workingDirectory);
            var currentBranch = repo.Branches[repo.Head.FriendlyName];
            var aheadBy = currentBranch.TrackingDetails.AheadBy;

            Application.Current.Dispatcher.Invoke(() =>
            {
                Label = aheadBy != null && aheadBy > 0 ? $"Push {aheadBy}↑" : "Push";
            });
        }

        protected override void WhenRuleApplicationClosed(object sender, EventArgs e)
        {
            if (RuleApplicationService.PersistenceInfo.IsGitRepository())
            {
                ((GitRuleApplicationServiceImpl)RuleApplicationService.Implementation).RuleApplicationCommitted -= PushCommand_RuleApplicationCommitted;
            }

            Label = "Push";
            IsEnabled = false;
        }

        public override void Execute()
        {
            var waitWindow = new BackgroundWorkerWaitWindow("Pushing to Git Repository", "Pushing changes to remote Git repository...");
            waitWindow.DoWork += delegate
            {
                var path = ((GitPersistenceInfo)RuleApplicationService.PersistenceInfo).Filename;
                using (var repo = InRuleGitRepository.Open(path))
                {
                    repo.Push(new PushOptions
                    {
                        CredentialsProvider = GitCredentialsProvider.CredentialsHandler
                    });
                }
            };

            waitWindow.RunWorkerCompleted += delegate (object sender1, RunWorkerCompletedEventArgs e1)
            {
                UpdateLabel();
            };

            waitWindow.ShowDialog();

            waitWindow.Close();
        }
    }
}
