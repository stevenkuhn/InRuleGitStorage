﻿namespace Sknet.InRuleGitStorage.AuthoringExtension.Commands;

public class MergeCommand : VisualCommandBase
{
    public MergeCommand()
        : base("[Git].[Actions].MergeCommand",
            "Merge",
            ImageFactory.GetImageThisAssembly("Images\\GitMerge16.png"),
            ImageFactory.GetImageThisAssembly("Images\\GitMerge32.png"),
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
