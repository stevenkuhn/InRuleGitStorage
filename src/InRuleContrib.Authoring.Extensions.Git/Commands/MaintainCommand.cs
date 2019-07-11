using InRule.Authoring.Commanding;
using InRule.Authoring.Media;

namespace InRuleContrib.Authoring.Extensions.Git.Commands
{
    public class MaintainCommand : VisualCommandBase
    {
        public MaintainCommand()
            : base("[Git].[Actions].Maintain",
                "Maintain",
                ImageFactory.GetImageAuthoringAssembly("/Images/Catalog16.png"),
                ImageFactory.GetImageAuthoringAssembly("/Images/Catalog32.png"),
                isEnabled: true)
        {
        }

        public override void Execute()
        {
        }
    }
}
