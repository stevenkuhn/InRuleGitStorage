using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Common.Utilities;
using InRule.Repository;
using System;

namespace Sknet.InRuleGitStorage.AuthoringExtension.Commands
{
    public class SaveToGitRepositoryCommand : VisualCommandBase
    {
        public SaveToGitRepositoryCommand()
            : base("SaveToGitRepository",
                "Save to Git Repository",
                ImageFactory.GetImageThisAssembly("Images\\SaveToGit16.png"),
                ImageFactory.GetImageThisAssembly("Images\\SaveToGit32.png"),
                isEnabled: false)
        {
            Subscribe(
                Subscription.RuleApplicationOpened,
                Subscription.RuleApplicationChanged,
                Subscription.RuleApplicationClosed);
        }

        protected override void WhenRuleApplicationOpened(object sender, EventArgs e)
        {
            IsEnabled = RuleApplicationService.RuleApplicationDef != null;
        }

        protected override void WhenRuleApplicationDefChanged(object sender, EventArgs<RuleApplicationDef> e)
        {
        }

        protected override void WhenRuleApplicationClosed(object sender, EventArgs e)
        {
            IsEnabled = RuleApplicationService.RuleApplicationDef != null;
        }

        public override void Execute()
        {
            //throw new NotImplementedException();
        }
    }
}
