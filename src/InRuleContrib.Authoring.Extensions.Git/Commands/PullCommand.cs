using InRule.Authoring.Commanding;
using InRule.Authoring.Media;

namespace InRuleContrib.Authoring.Extensions.Git.Commands
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
        }

        public override void Execute()
        {
        }
    }
}
