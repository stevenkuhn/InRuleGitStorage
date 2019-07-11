using InRule.Authoring.Commanding;
using InRule.Authoring.Media;

namespace InRuleContrib.Authoring.Extensions.Git.Commands
{
    public class BranchCommand : VisualCommandBase
    {
        public BranchCommand()
            : base("[Git].[Actions].BranchCommand",
                "Branch",
                ImageFactory.GetImageThisAssembly("Images\\GitBranch16.png"),
                ImageFactory.GetImageThisAssembly("Images\\GitBranch32.png"),
                isEnabled: false)
        {
        }

        public override void Execute()
        {
        }
    }
}
