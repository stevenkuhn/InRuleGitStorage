using InRule.Authoring.Commanding;
using InRule.Authoring.Media;

namespace InRuleContrib.Authoring.Extensions.Git.Commands
{
    public class DiscardCommand : VisualCommandBase
    {
        public DiscardCommand()
            : base("[Git].[Actions].DiscardCommand",
                "Discard Changes",
                ImageFactory.GetImageThisAssembly("Images\\GitDiscard16.png"),
                ImageFactory.GetImageThisAssembly("Images\\GitDiscard32.png"),
                isEnabled: false)
        {
        }

        public override void Execute()
        {
        }
    }
}
