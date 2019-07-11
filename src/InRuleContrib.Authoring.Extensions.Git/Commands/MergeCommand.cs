using InRule.Authoring.Commanding;
using InRule.Authoring.Media;

namespace InRuleContrib.Authoring.Extensions.Git.Commands
{
    public class MergeCommand : VisualCommandBase
    {
        public MergeCommand()
            : base("[Git].[Actions].MergeCommand",
                "Merge",
                ImageFactory.GetImageThisAssembly("Images\\GitMerge16.png"),
                ImageFactory.GetImageThisAssembly("Images\\GitMerge32.png"),
                isEnabled: false)
        {
        }

        public override void Execute()
        {
        }
    }
}
