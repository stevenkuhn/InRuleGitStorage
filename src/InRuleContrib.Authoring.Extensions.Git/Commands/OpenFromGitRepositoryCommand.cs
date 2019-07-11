using InRule.Authoring.Commanding;
using InRule.Authoring.Media;
using InRule.Common.Utilities;
using InRule.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

    public class PushCommand : VisualCommandBase
    {
        public PushCommand()
            : base("[Git].[Actions].PushCommand",
                "Push",
                ImageFactory.GetImageThisAssembly("Images\\GitPush16.png"),
                ImageFactory.GetImageThisAssembly("Images\\GitPush32.png"),
                isEnabled: false)
        {
        }

        public override void Execute()
        {
        }
    }

    public class PullCommand : VisualCommandBase
    {
        public PullCommand()
            : base("[Git].[Actions].PullCommand",
                "Pull",
                ImageFactory.GetImageThisAssembly("Images\\GitPull16.png"),
                ImageFactory.GetImageThisAssembly("Images\\GitPull32.png"),
                isEnabled: false)
        {
        }

        public override void Execute()
        {
        }
    }

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
            RuleApplicationService.OpenFromFile("git://");
        }
    }
}
