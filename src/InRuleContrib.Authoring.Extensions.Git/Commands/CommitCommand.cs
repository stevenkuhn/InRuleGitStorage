using InRule.Authoring.Commanding;
using InRule.Authoring.Media;

namespace InRuleContrib.Authoring.Extensions.Git.Commands
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
        }

        public override void Execute()
        {
        }
    }
}
