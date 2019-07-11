using InRule.Authoring;
using InRule.Authoring.Media;
using InRule.Authoring.Services;
using InRule.Authoring.Windows;
using InRule.Common.Utilities;
using InRule.Repository;
using InRuleContrib.Authoring.Extensions.Git.Controls;
using InRuleContrib.Authoring.Extensions.Git.ViewModels;
using InRuleContrib.Repository.Storage.Git;
using LibGit2Sharp;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;

namespace InRuleContrib.Authoring.Extensions.Git
{
    public class GitRuleApplicationServiceImpl : RuleApplicationServiceWrapper
    {
        public RuleApplicationService RuleApplicationService { get; set; }
        public ServiceManager ServiceManager { get; set; }

        public GitRuleApplicationServiceImpl(IRuleApplicationServiceImplementation innerRuleApplicationServiceImpl)
            : base(innerRuleApplicationServiceImpl)
        {
        }

        public GitRepositoryOption GetSelectedGitRepositoryOption()
        {
            GitRepositoryOption selectedOption = null;

            var control = new GitRepositoryOptionControl();
            var viewModel = ServiceManager.Compose<GitRepositoryOptionControlViewModel>();
            viewModel.View = control;

            control.DataContext = viewModel;

            var window = WindowFactory.CreateWindow("Git Repositories", control, "Close");
            ((Window)window).Icon = ImageFactory.GetImageThisAssembly("Images\\Git16.png");
            ((Window)window).MinWidth = 634;

            viewModel.OwningWindow = (Window)window;

            viewModel.UseThisClicked += delegate (object sender, EventArgs e)
            {
                var selectedOptionViewModel = ((EventArgs<GitRepositoryOptionViewModel>)e).Item;
                var close = true;

                var waitWindow = new BackgroundWorkerWaitWindow("Cloning repository", "Cloning remote Git repository...");
                waitWindow.DoWork += delegate
                {
                    if (InRuleGitRepository.IsValid(selectedOptionViewModel.Model.WorkingDirectory))
                    {
                    }
                    else if (Directory.EnumerateFileSystemEntries(selectedOptionViewModel.Model.WorkingDirectory).Any())
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        var path = InRuleGitRepository.Clone(
                            sourceUrl: selectedOptionViewModel.SourceUrl, 
                            destinationPath: selectedOptionViewModel.Model.WorkingDirectory,
                            options: new InRuleContrib.Repository.Storage.Git.CloneOptions
                            {
                                CredentialsProvider = (url, usernameFromUrl, types) => new UsernamePasswordCredentials
                                {
                                    Username = selectedOptionViewModel.Username,
                                    Password = selectedOptionViewModel.Password
                                }
                            }
                        );
                        var isValid = InRuleGitRepository.IsValid(path);
                    }

                    selectedOption = selectedOptionViewModel.Model;
                };

                waitWindow.RunWorkerCompleted += delegate (object sender1, RunWorkerCompletedEventArgs e1)
                {
                    if (e1.Error != null)
                    {
                        throw new NotImplementedException();
                    }
                };

                waitWindow.ShowDialog();

                if (close)
                {
                    window.Close();
                }
            };

            window.ButtonClicked += delegate (object sender, WindowButtonClickedEventArgs<GitRepositoryOptionControl> e)
            {
                viewModel.SaveSettings();
                window.Close();
            };

            viewModel.LoadSettings();
            window.Show();

            return selectedOption;
        }

        public override bool OpenFromFilename(string filename)
        {
            if (filename != "git://")
            {
                return base.OpenFromFilename(filename);
            }

            var selectedOption = GetSelectedGitRepositoryOption();

            if (selectedOption == null)
            {
                return false;
            }

            var success = true;
            RuleApplicationDef ruleAppDef = null;
            //var appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //var gitRepositoryPath = Path.Combine(appDataDirectory, "InRule", "irAuthor", "GitRepository");

            var window = new BackgroundWorkerWaitWindow(Strings.Open_Rule_Application, "Loading from Git Repository...");

            window.DoWork += delegate
            {
                if (!InRuleGitRepository.IsValid(selectedOption.WorkingDirectory))
                {
                    throw new NotImplementedException();
                }

                // open up repo and get rule applications

                using (var repository = InRuleGitRepository.Open(selectedOption.WorkingDirectory))
                {
                    var ruleApplications = repository.GetRuleApplications();
                    //var ruleApplications = repository.GetRuleApplicationSummaries();
                    //ruleAppDef = repository.GetRuleApplication("NewRuleApplication");
                    ruleAppDef = repository.GetRuleApplication(ruleApplications[0].Name);
                }
            };

            window.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error == null && ruleAppDef != null)
                {
                    RuleApplicationService.SetRuleApplicationDef(ruleAppDef, new GitPersistenceInfo(ruleAppDef.Name, selectedOption.WorkingDirectory));
                    return;
                }

                if (e.Error != null)
                {
                    throw new NotImplementedException();
                }
            };

            window.ShowDialog();

            return success;
        }

        public override bool Save()
        {
            var persistenceInfo = RuleApplicationService.PersistenceInfo;

            if (persistenceInfo == null)
            {
                return base.Save();
            }

            if (persistenceInfo.IsGitRepository())
            {
                return SaveToGitRepository();
            }

            if (persistenceInfo?.OpenedFrom == RuleAppOpenedFrom.New)
            {
                var taskDialog = new TaskDialog
                {
                    MainInstruction = Strings.Save_Rule_Application,
                    MainIcon = TaskDialogIcon.Information,
                    Content = Strings.Where_would_you_like_to_save_the_rule_application_,
                    CommandLinks =
                    {
                        new TaskDialogCommandLink(Strings.Catalog, Strings.Save_the_rule_application_to_an_InRule_Catalog),
                        new TaskDialogCommandLink(Strings.File, Strings.Save_the_rule_application_to_the_file_system),
                        new TaskDialogCommandLink("Git Repository", "Save the rule application to a Git repository")
                    },
                    CommonButtons = TaskDialogCommonButtons.Cancel
                };

                var result = taskDialog.Show();

                switch (result.CommandLinkResult)
                {
                    case 0:
                        return SaveToCatalog(false);
                    case 1:
                        return SaveToFile();
                    case 2:
                        return SaveToGitRepository();
                }
            }

            return base.Save();
        }

        public bool SaveToGitRepository()
        {
            var success = false;
            var ruleAppDef = RuleApplicationService.RuleApplicationDef;

            if (ruleAppDef == null)
            {
                return false;
            }

            try
            {
                var selectedOption = GetSelectedGitRepositoryOption();

                if (selectedOption == null)
                {
                    return false;
                }

                if (!InRuleGitRepository.IsValid(selectedOption.WorkingDirectory))
                {
                    throw new NotImplementedException();
                }

                using (var repository = InRuleGitRepository.Open(selectedOption.WorkingDirectory))
                {
                    var identity = new Identity("Steven Kuhn", "email@stevenkuhn.net");
                    var signature = new Signature(identity, DateTimeOffset.UtcNow);

                    repository.Commit(ruleAppDef, $"Saving rule app {ruleAppDef.Name}", signature, signature);
                    ruleAppDef.SetOriginalContentCode();
                }

                /*var appDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var gitRepositoryPath = Path.Combine(appDataDirectory, "InRule", "irAuthor", "GitRepository");

                if (!InRuleGitRepository.IsValid(gitRepositoryPath))
                {
                    InRuleGitRepository.Init(gitRepositoryPath);
                }

                using (var repository = InRuleGitRepository.Open(gitRepositoryPath))
                {
                    var identity = new Identity("Steven Kuhn", "email@stevenkuhn.net");
                    var signature = new Signature(identity, DateTimeOffset.UtcNow);

                    repository.Commit(ruleAppDef, $"Saving rule app {ruleAppDef.Name}", signature, signature);
                    ruleAppDef.SetOriginalContentCode();
                }*/

                if (ruleAppDef.CatalogState != CatalogState.None)
                {
                    ruleAppDef.VisitDefs(delegate (RuleRepositoryDefBase def)
                    {
                        if (def is RuleApplicationDef)
                        {
                            ((RuleApplicationDef)def).SchemaCatalogState = CatalogState.None;
                            ((RuleApplicationDef)def).SchemaCatalogSharingState =
                                CatalogSharingState.None;

                        }
                        def.CatalogState = CatalogState.None;
                        def.CatalogSharingState = CatalogSharingState.None;
                        return true;
                    });
                }

                var newPersistenceInfo = new GitPersistenceInfo(ruleAppDef.Name, selectedOption.WorkingDirectory);

                if (!RuleApplicationService.PersistenceInfo.IsSame(newPersistenceInfo))
                {
                    RuleApplicationService.SetRuleApplicationDef(ruleAppDef, newPersistenceInfo);
                }

                success = true;
            }
            catch (Exception e)
            {
                var text = string.Format(Strings.The_following_error_occurred___0, e.Message);
                MessageBoxFactory.Show(text, "Error saving rule application", MessageBoxFactoryImage.Error);
            }

            return success;
        }
    }
}
