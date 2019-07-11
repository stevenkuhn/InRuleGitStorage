using InRule.Authoring.Commanding;
using InRule.Authoring.Media;

namespace InRuleContrib.Authoring.Extensions.Git.Commands
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
        }

        public override void Execute()
        {
        }
    }
}
