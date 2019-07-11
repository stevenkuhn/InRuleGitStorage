using InRule.Authoring.Commanding;
using InRule.Authoring.Media;

namespace InRuleContrib.Authoring.Extensions.Git.Commands
{
    public class LogoutCommand : VisualCommandBase
    {
        public LogoutCommand()
            : base("[Git].[Actions].LogOut",
                "Log Out",
                ImageFactory.GetImageAuthoringAssembly("/Images/Catalog16.png"),
                ImageFactory.GetImageAuthoringAssembly("/Images/LogOut32.png"),
                isEnabled: false)
        {
        }

        public override void Execute()
        {
        }
    }
}
