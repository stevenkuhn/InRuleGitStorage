using InRule.Authoring.Extensions;
using InRule.Authoring.Media;
using InRule.Authoring.Services;
using InRule.Authoring.Windows;
using InRule.Authoring.Windows.Controls;
using InRule.Common.Utilities;
using InRule.Repository;
using Sknet.InRuleGitStorage.AuthoringExtension.Commands;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Sknet.InRuleGitStorage.AuthoringExtension
{
    public class Extension : ExtensionBase
    {
        private IRibbonTab _tab;
        private IRibbonMenuButton _fileOpenMenu;
        private IRibbonMenuButton _fileSaveAsMenu;
        private IRibbonButton _openFromGitRepoButton;
        private IRibbonButton _saveToGitRepoButton;
        private StatusBarItem _gitRepositoryStatusBarItem;

        public Extension() : base(
            name: "Sknet.InRuleGitStorage",
            description: "Stores rule applications in a git repository",
            guid: new Guid("{2D6E8F02-B55D-44FD-95C5-609B074FA7BE}"),
            isSystemExtension: false)
        {

        }

        public override void Enable()
        {
            var originalRuleApplicationServiceImpl = RuleApplicationService.Implementation;
            var gitRuleApplicationServiceImpl = ServiceManager.Compose<GitRuleApplicationServiceImpl>(ServiceManager, originalRuleApplicationServiceImpl);
            RuleApplicationService.Implementation = gitRuleApplicationServiceImpl;

            RuleApplicationService.RuleApplicationDefChanged += WhenRuleApplicationDefChanged;

            _tab = IrAuthorShell.Ribbon.AddTab("Git");

            var applicationMenu = IrAuthorShell.Ribbon.ApplicationMenu.Items
                .AsGeneric<object>()
                .Where(x => x is IRibbonMenuButton)
                .Cast<IRibbonMenuButton>()
                .ToDictionary(x => x.AutomationId);

            _fileOpenMenu = applicationMenu["File_Open"];
            var openFromGitRepoCommand = ServiceManager.Compose<OpenFromGitRepositoryCommand>();
            _openFromGitRepoButton = _fileOpenMenu.InsertMenuItem(3,
                openFromGitRepoCommand,
                "Open a rule application from a Git repository");

            _fileSaveAsMenu = applicationMenu["File_SaveAs"];
            var saveToGitRepoCommand = ServiceManager.Compose<SaveToGitRepositoryCommand>();
            _saveToGitRepoButton = _fileSaveAsMenu.AddMenuItem(
                saveToGitRepoCommand,
                "Save the rule application to a Git repository");

            var group = _tab.AddGroup("Rule Application", null, "");

            var commitCommand = ServiceManager.Compose<CommitCommand>();
            group.AddButton(commitCommand);

            var discardCommand = ServiceManager.Compose<DiscardCommand>();
            group.AddButton(discardCommand);

            group = _tab.AddGroup("Branches", null, "");

            var branchCommand = ServiceManager.Compose<BranchCommand>();
            group.AddButton(branchCommand);

            var mergeCommand = ServiceManager.Compose<MergeCommand>();
            group.AddButton(mergeCommand);

            group = _tab.AddGroup("Repository", null, "");

            var pullCommand = ServiceManager.Compose<PullCommand>();
            group.AddButton(pullCommand);

            var pushCommand = ServiceManager.Compose<PushCommand>();
            group.AddButton(pushCommand);

            group = _tab.AddGroup("General", null, "");

            var logoutCommand = ServiceManager.Compose<LogoutCommand>();
            group.AddButton(logoutCommand);

            var maintainCommand = ServiceManager.Compose<MaintainCommand>();
            group.AddButton(maintainCommand);
        }

        private void WhenRuleApplicationDefChanged(object sender, EventArgs<RuleApplicationDef> e)
        {
            IrAuthorShell.StatusBar.Items.Remove(_gitRepositoryStatusBarItem);

            var ruleAppDef = RuleApplicationService.RuleApplicationDef;

            if (ruleAppDef == null)
            {
                return;
            }

            var persistenceInfo = RuleApplicationService.PersistenceInfo as GitPersistenceInfo;

            if (persistenceInfo == null)
            {
                return;
            }

            var statusImage = new Image { Source = ImageFactory.GetImageThisAssembly("Images\\Git16.png"), Height = 16, Width = 16, VerticalAlignment = VerticalAlignment.Center };
            var statusTextBox = new TextBlock { Text = persistenceInfo.RepositoryOption.SourceUrl, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(5, 0, 0, 0), TextTrimming = TextTrimming.WordEllipsis };

            var stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            stackPanel.Children.Add(statusImage);
            stackPanel.Children.Add(statusTextBox);
            stackPanel.Children.Add(new Separator { Margin = new Thickness(5, 0, 0, 0) });

            _gitRepositoryStatusBarItem = new StatusBarItem { Content = stackPanel, VerticalAlignment = VerticalAlignment.Center };

            var firstItem = IrAuthorShell.StatusBar.Items[0];

            IrAuthorShell.StatusBar.Items.Clear();
            IrAuthorShell.StatusBar.Items.Add(firstItem);
            IrAuthorShell.StatusBar.Items.Add(_gitRepositoryStatusBarItem);
        }

        public override void Disable()
        {
            IrAuthorShell.Ribbon.RemoveTab(_tab);
            _tab = null;

            _fileOpenMenu.RemoveMenuItem(_openFromGitRepoButton);
            _fileOpenMenu = null;
            _openFromGitRepoButton = null;

            _fileSaveAsMenu.RemoveMenuItem(_saveToGitRepoButton);
            _fileSaveAsMenu = null;
            _saveToGitRepoButton = null;

            // TODO: what happens when rule application from git is still loaded?
            RuleApplicationService.RuleApplicationDefChanged -= WhenRuleApplicationDefChanged;

            var gitRuleApplicationServiceImpl = (GitRuleApplicationServiceImpl)RuleApplicationService.Implementation;
            RuleApplicationService.Implementation = gitRuleApplicationServiceImpl.InnerImplementation;
            gitRuleApplicationServiceImpl.Dispose();
        }
    }
}
