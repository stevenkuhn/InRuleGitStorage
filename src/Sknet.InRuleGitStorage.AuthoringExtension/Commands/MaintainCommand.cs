using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Common.Utilities;
using InRule.Repository;
using System;

namespace Sknet.InRuleGitStorage.AuthoringExtension.Commands
{
    public class MaintainCommand : VisualCommandBase
    {
        public MaintainCommand()
            : base("[Git].[Actions].Maintain",
                "Maintain",
                ImageFactory.GetImageAuthoringAssembly("/Images/Catalog16.png"),
                ImageFactory.GetImageAuthoringAssembly("/Images/Catalog32.png"),
                isEnabled: false)
        {
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
            //IsEnabled = false;
        }

        public override void Execute()
        {
        }
    }
}
