using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Common.Utilities;
using InRule.Repository;
using System;

namespace Sknet.InRuleGitStorage.AuthoringExtension.Commands
{
    public class CommitCommand : VisualCommandBase
    {
        public CommitCommand()
            : base("[Git].[Actions].CommitCommand",
                "Save",
                ImageFactory.GetImageThisAssembly("Images\\GitCommit16.png"),
                ImageFactory.GetImageThisAssembly("Images\\GitCommit32.png"),
                isEnabled: false)
        {
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
            IsEnabled = RuleApplicationService.PersistenceInfo.IsGitRepository();
        }

        protected override void WhenRuleApplicationClosed(object sender, EventArgs e)
        {
            IsEnabled = false;
        }

        public override void Execute()
        {
        }
    }
}
