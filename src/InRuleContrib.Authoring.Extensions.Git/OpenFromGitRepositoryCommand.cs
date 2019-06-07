using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Common.Utilities;
using InRule.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InRuleContrib.Authoring.Extensions.Git
{
    public class OpenFromGitRepositoryCommand : VisualCommandBase
    {
        public OpenFromGitRepositoryCommand()
            : base("OpenFromGitRepository",
                "Open from Git Repository",
                ImageFactory.GetImageThisAssembly("Images\\OpenFromGit16.png"),
                ImageFactory.GetImageThisAssembly("Images\\OpenFromGit32.png"),
                isEnabled: true)
        {
            Subscribe(
                Subscription.RuleApplicationOpened,
                Subscription.RuleApplicationChanged,
                Subscription.RuleApplicationClosed);
        }

        protected override void WhenRuleApplicationOpened(object sender, EventArgs e)
        {
        }

        protected override void WhenRuleApplicationDefChanged(object sender, EventArgs<RuleApplicationDef> e)
        {
        }

        protected override void WhenRuleApplicationClosed(object sender, EventArgs e)
        {
        }

        public override void Execute()
        {
            throw new NotImplementedException();
        }
    }
}
