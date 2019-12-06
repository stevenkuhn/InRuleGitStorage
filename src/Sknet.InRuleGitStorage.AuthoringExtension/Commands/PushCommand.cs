using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Common.Utilities;
using InRule.Repository;
using System;

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
            //IsEnabled = RuleApplicationService.PersistenceInfo.IsGitRepository();
        }

        protected override void WhenRuleApplicationDefChanged(object sender, EventArgs<RuleApplicationDef> e)
        {
            //IsEnabled = RuleApplicationService.PersistenceInfo.IsGitRepository();
        }

        protected override void WhenRuleApplicationClosed(object sender, EventArgs e)
        {
            IsEnabled = false;
        }

        public override void Execute()
        {
            var path = ((GitPersistenceInfo)RuleApplicationService.PersistenceInfo).Filename;
            using (var repo = InRuleGitRepository.Open(path))
            {
                repo.Push(new PushOptions
                {
                    CredentialsProvider = GitCredentialsProvider.CredentialsHandler
                });
            }
        }
    }
}
