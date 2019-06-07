using InRule.Authoring.Extensions;
using InRule.Authoring.Windows;
using InRule.Authoring.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InRuleContrib.Authoring.Extensions.Git
{
    public class Extension : ExtensionBase
    {
        //private IRibbonTab _tab;
        private IRibbonMenuButton _fileOpenMenu;
        private IRibbonMenuButton _fileSaveAsMenu;
        private IRibbonButton _openFromGitRepoButton;
        private IRibbonButton _saveToGitRepoButton;

        public Extension() : base(
            name: "InRuleContrib.Authoring.Extensions.Git",
            description: "Stores rule applications in a git repository",
            guid: new Guid("{2D6E8F02-B55D-44FD-95C5-609B074FA7BE}"),
            isSystemExtension : false)
        {
            
        }

        public override void Enable()
        {
            //_tab = IrAuthorShell.Ribbon.AddTab("Git");

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

            //var group = _tab.AddGroup("Actions", null, "");
        }

        public override void Disable()
        {
            //IrAuthorShell.Ribbon.RemoveTab(_tab);
            //_tab = null;

            _fileOpenMenu.RemoveMenuItem(_openFromGitRepoButton);
            _fileOpenMenu = null;
            _openFromGitRepoButton = null;

            _fileSaveAsMenu.RemoveMenuItem(_saveToGitRepoButton);
            _fileSaveAsMenu = null;
            _saveToGitRepoButton = null;
        }
    }
}
