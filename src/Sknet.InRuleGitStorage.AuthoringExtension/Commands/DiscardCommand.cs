namespace Sknet.InRuleGitStorage.AuthoringExtension.Commands;

public class DiscardCommand : VisualCommandBase
{
    public DiscardCommand()
        : base("[Git].[Actions].DiscardCommand",
            "Discard Changes",
            ImageFactory.GetImageThisAssembly("Images\\GitDiscard16.png"),
            ImageFactory.GetImageThisAssembly("Images\\GitDiscard32.png"),
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
        //IsEnabled = false;
    }

    public override void Execute()
    {
    }
}
