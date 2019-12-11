using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Common.Utilities;
using InRule.Repository;
using System;

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
        }
    }
}
