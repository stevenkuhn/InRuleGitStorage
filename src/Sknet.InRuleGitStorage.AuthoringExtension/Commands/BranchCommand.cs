namespace Sknet.InRuleGitStorage.AuthoringExtension.Commands;

public class BranchCommand : VisualCommandBase
{
    public BranchCommand()
        : base("[Git].[Actions].BranchCommand",
            "Branch",
            ImageFactory.GetImageThisAssembly("Images\\GitBranch16.png"),
            ImageFactory.GetImageThisAssembly("Images\\GitBranch32.png"),
            isEnabled: false)
    {
        Subscribe(
            Subscription.RuleApplicationOpened,
            Subscription.RuleApplicationChanged,
            Subscription.RuleApplicationClosed);
    }

    protected override void WhenRuleApplicationOpened(object sender, EventArgs e)
    {
        //IsEnabled = RuleApplicationService.PersistenceInfo.IsGitRepository();
    }

    protected override void WhenRuleApplicationDefChanged(object sender, EventArgs<RuleApplicationDef> e)
    {
        //IsEnabled = RuleApplicationService.PersistenceInfo.IsGitRepository();
    }

    protected override void WhenRuleApplicationClosed(object sender, EventArgs e)
    {
        IsEnabled = false;
    }

    public override void Execute()
    {
    }
}
