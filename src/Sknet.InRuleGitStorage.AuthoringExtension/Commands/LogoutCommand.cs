namespace Sknet.InRuleGitStorage.AuthoringExtension.Commands;

public class LogoutCommand : VisualCommandBase
{
    public LogoutCommand()
        : base("[Git].[Actions].LogOut",
            "Log Out",
            ImageFactory.GetImageAuthoringAssembly("/Images/Catalog16.png"),
            ImageFactory.GetImageAuthoringAssembly("/Images/LogOut32.png"),
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
