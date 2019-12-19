using InRule.Authoring;
using InRule.Authoring.Media;
using InRule.Authoring.Services;
using InRule.Authoring.Windows;
using InRule.Common.Utilities;
using InRule.Repository;
using InRule.Repository.SchemaOperations;
using Sknet.InRuleGitStorage.AuthoringExtension.Controls;
using Sknet.InRuleGitStorage.AuthoringExtension.ViewModels;
using LibGit2Sharp.Handlers;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows;
using InRule.Repository.Client;

namespace Sknet.InRuleGitStorage.AuthoringExtension
{
    public class GitRuleApplicationServiceImpl : RuleApplicationServiceWrapper, IDisposable
    {
        private readonly RuleApplicationService _ruleApplicationService;
        private readonly ServiceManager _serviceManager;

        private GitRepositoryOption _selectedGitRepositoryOption;

        public GitRuleApplicationServiceImpl(ServiceManager serviceManager, IRuleApplicationServiceImplementation innerRuleApplicationServiceImpl)
            : base(innerRuleApplicationServiceImpl)
        {
            _serviceManager = serviceManager;
            _ruleApplicationService = serviceManager.GetService<RuleApplicationService>();

            _ruleApplicationService.Closed += RuleApplicationService_Closed;
            _ruleApplicationService.Closing += RuleApplicationService_Closing;
            _ruleApplicationService.NewRuleApplicationCreated += RuleApplicationService_NewRuleApplicationCreated;
            _ruleApplicationService.Opened += RuleApplicationService_Opened;
            _ruleApplicationService.Opening += RuleApplicationService_Opening;
            _ruleApplicationService.RuleApplicationDefChanged += RuleApplicationService_RuleApplicationDefChanged;
            _ruleApplicationService.RuleApplicationDefChanging += RuleApplicationService_RuleApplicationDefChanging;
            _ruleApplicationService.ValidationFailed += RuleApplicationService_ValidationFailed;
        }

        public void Dispose()
        {
            _ruleApplicationService.Closed -= RuleApplicationService_Closed;
            _ruleApplicationService.Closing -= RuleApplicationService_Closing;
            _ruleApplicationService.NewRuleApplicationCreated -= RuleApplicationService_NewRuleApplicationCreated;
            _ruleApplicationService.Opened -= RuleApplicationService_Opened;
            _ruleApplicationService.Opening -= RuleApplicationService_Opening;
            _ruleApplicationService.RuleApplicationDefChanged -= RuleApplicationService_RuleApplicationDefChanged;
            _ruleApplicationService.RuleApplicationDefChanging -= RuleApplicationService_RuleApplicationDefChanging;
            _ruleApplicationService.ValidationFailed -= RuleApplicationService_ValidationFailed;
        }

        public event EventHandler RuleApplicationCommitted;

        private GitRepositoryOption GetSelectedGitRepositoryOption()
        {
            if (_selectedGitRepositoryOption != null)
            {
                return _selectedGitRepositoryOption;
            }

            GitRepositoryOption selectedOption = null;

            var control = new GitRepositoryOptionControl();
            var viewModel = _serviceManager.Compose<GitRepositoryOptionControlViewModel>();
            viewModel.View = control;

            control.DataContext = viewModel;

            var window = WindowFactory.CreateWindow("Git Repositories", control, "Close");
            ((Window)window).Icon = ImageFactory.GetImageThisAssembly("Images\\Git16.png");
            ((Window)window).MinWidth = 634;

            viewModel.OwningWindow = (Window)window;

            viewModel.UseThisClicked += delegate (object sender, EventArgs e)
            {
                selectedOption = ((EventArgs<GitRepositoryOptionViewModel>)e).Item.Model;

                BackgroundWorkerWaitWindow waitWindow = null;

                if (InRuleGitRepository.IsValid(selectedOption.WorkingDirectory))
                {
                    waitWindow = new BackgroundWorkerWaitWindow("Connect to Git Repository", "Connecting to remote Git repository...");
                    waitWindow.DoWork += delegate
                    {
                        using (var repository = InRuleGitRepository.Open(selectedOption.WorkingDirectory))
                        {
                            repository.Fetch(new FetchOptions
                            {
                                CredentialsProvider = GitCredentialsProvider.CredentialsHandler
                            });
                        }
                    };
                }
                else if (Directory.Exists(selectedOption.WorkingDirectory) && Directory.EnumerateFileSystemEntries(selectedOption.WorkingDirectory).Any())
                {
                    // Working directory is not empty, but it is not a valid InRule git repository
                    throw new NotImplementedException();
                }
                else
                {
                    // working directory is empty or does not exist
                    waitWindow = new BackgroundWorkerWaitWindow("Connect to Git Repository", "Connecting to remote Git repository...");
                    waitWindow.DoWork += delegate
                    {
                        var test = new LibGit2Sharp.DefaultCredentials();

                        InRuleGitRepository.Clone(
                            sourceUrl: selectedOption.SourceUrl,
                            destinationPath: selectedOption.WorkingDirectory,
                            options: new CloneOptions
                            {
                                CredentialsProvider = GitCredentialsProvider.CredentialsHandler
                            });
                    };
                }

                waitWindow.RunWorkerCompleted += delegate (object sender1, RunWorkerCompletedEventArgs e1)
                {
                    if (e1.Error != null)
                    {
                        throw new NotImplementedException();
                    }
                };

                waitWindow.ShowDialog();

                window.Close();
            };

            window.ButtonClicked += delegate (object sender, WindowButtonClickedEventArgs<GitRepositoryOptionControl> e)
            {
                viewModel.SaveSettings();
                window.Close();
            };

            viewModel.LoadSettings();
            window.Show();

            _selectedGitRepositoryOption = selectedOption;

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

            RuleApplicationGitInfo[] ruleApplications = null;

            var waitWindow = new BackgroundWorkerWaitWindow("Open from Git Repository", "Retrieving rule applications...");
            waitWindow.DoWork += delegate
            {
                using (var repository = InRuleGitRepository.Open(selectedOption.WorkingDirectory))
                {
                    ruleApplications = repository.GetRuleApplications();
                }
            };

            waitWindow.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
            {
                if (e.Error != null)
                {
                    throw new NotImplementedException();
                }
            };

            waitWindow.ShowDialog();

            if (ruleApplications == null)
            {
                throw new NotImplementedException();
            }

            RuleApplicationGitInfo selectedRuleAppInfo = null;

            var control = _serviceManager.Compose<OpenFromGitRepositoryControl>(selectedOption, ruleApplications);
            var window = WindowFactory.CreateWindow("Open from Git Repository", control, 800, 350, true);
            window.ButtonClicked += delegate (object sender, WindowButtonClickedEventArgs<OpenFromGitRepositoryControl> e)
            {
                if (e.ClickedButtonText == Strings.Open)
                {
                    selectedRuleAppInfo = control.SelectedRuleApplicationGitInfo;
                }

                window.Close();
            };

            window.Show();

            if (selectedRuleAppInfo != null)
            {
                RuleApplicationDef ruleAppDef = null;
                var backgroundWindow = new BackgroundWorkerWaitWindow(Strings.Open_Rule_Application, "Loading from Git repository...");

                backgroundWindow.DoWork += delegate
                {
                    using (var repository = InRuleGitRepository.Open(selectedOption.WorkingDirectory))
                    {
                        ruleAppDef = repository.GetRuleApplication(selectedRuleAppInfo.Name);
                        ruleAppDef.SetOriginalContentCode();
                    }
                };

                backgroundWindow.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
                {
                    if (e.Error == null && ruleAppDef != null)
                    {
                        var existingPersistenceInfo = _ruleApplicationService.PersistenceInfo;
                        var newPersistenceInfo = new GitPersistenceInfo(ruleAppDef.Name, selectedOption.WorkingDirectory, selectedOption);

                        if (existingPersistenceInfo == null || !existingPersistenceInfo.IsSame(newPersistenceInfo))
                        {
                            _ruleApplicationService.SetRuleApplicationDef(ruleAppDef, newPersistenceInfo);
                        }

                        return;
                    }

                    if (e.Error != null)
                    {
                        throw new NotImplementedException();
                    }
                };

                backgroundWindow.ShowDialog();

                return true;
            }

            return false;
        }

        // HACK
        public override bool CheckOut(RuleAppCheckOutMode checkOutMode)
        {
            if (!_ruleApplicationService.PersistenceInfo.IsGitRepository())
            {
                return base.CheckOut(checkOutMode);
            }

            var waitWindow = new BackgroundWorkerWaitWindow("Pulling from Git Repository", "Pulling changes from remote Git repository...");
            waitWindow.DoWork += delegate
            {
                var path = ((GitPersistenceInfo)_ruleApplicationService.PersistenceInfo).Filename;
                using (var repo = InRuleGitRepository.Open(path))
                {
                    repo.Pull(new PullOptions
                    {
                        FetchOptions = new FetchOptions
                        {
                            CredentialsProvider = GitCredentialsProvider.CredentialsHandler
                        }
                    });

                    var ruleAppDef = repo.GetRuleApplication(_ruleApplicationService.RuleApplicationDef.Name);
                    ruleAppDef.SetOriginalContentCode();

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _ruleApplicationService.SetRuleApplicationDef(ruleAppDef, _ruleApplicationService.PersistenceInfo);
                    });
                }
            };

            waitWindow.RunWorkerCompleted += delegate (object sender1, RunWorkerCompletedEventArgs e1)
            {

            };

            waitWindow.ShowDialog();

            waitWindow.Close();

            return true;
        }

        public override bool Save()
        {
            var persistenceInfo = _ruleApplicationService.PersistenceInfo;

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
            var ruleAppDef = _ruleApplicationService.RuleApplicationDef;

            if (ruleAppDef == null)
            {
                return false;
            }

            var success = false;

            try
            {
                var selectedOption = GetSelectedGitRepositoryOption();

                if (selectedOption == null)
                {
                    return false;
                }

                var waitWindow = new BackgroundWorkerWaitWindow("Save to Git Repository", "Saving to Git repository...");
                waitWindow.DoWork += delegate
                {
                    if (!InRuleGitRepository.IsValid(selectedOption.WorkingDirectory))
                    {
                        throw new NotImplementedException();
                    }

                    using (var repository = InRuleGitRepository.Open(selectedOption.WorkingDirectory))
                    {
                        repository.Commit(ruleAppDef, $"Saving rule app {ruleAppDef.Name}");
                        ruleAppDef.SetOriginalContentCode();
                    }

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

                        var existingPersistenceInfo = _ruleApplicationService.PersistenceInfo;
                        var newPersistenceInfo = new GitPersistenceInfo(ruleAppDef.Name, selectedOption.WorkingDirectory, selectedOption);

                        if (existingPersistenceInfo == null || !existingPersistenceInfo.IsSame(newPersistenceInfo))
                        {
                            _ruleApplicationService.SetRuleApplicationDef(ruleAppDef, newPersistenceInfo);
                        }
                    }

                    RuleApplicationCommitted?.Invoke(this, new EventArgs());
                };

                waitWindow.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
                {
                    if (e.Error != null)
                    {
                        throw new NotImplementedException();
                    }
                };

                waitWindow.ShowDialog();
                waitWindow.Close();

                success = true;
            }
            catch (Exception e)
            {
                var text = string.Format(Strings.The_following_error_occurred___0, e.Message);
                MessageBoxFactory.Show(text, "Error saving rule application", MessageBoxFactoryImage.Error);
            }

            return success;
        }

        private void RuleApplicationService_Closed(object sender, EventArgs<RuleApplicationDef> e)
        {
        }

        private void RuleApplicationService_Closing(object sender, CancelEventArgs e)
        {
        }

        private void RuleApplicationService_NewRuleApplicationCreated(object sender, EventArgs e)
        {
        }

        private void RuleApplicationService_Opened(object sender, EventArgs e)
        {
        }

        private void RuleApplicationService_Opening(object sender, CancelEventArgs e)
        {
        }

        private void RuleApplicationService_RuleApplicationDefChanged(object sender, EventArgs<RuleApplicationDef> e)
        {
        }

        private void RuleApplicationService_RuleApplicationDefChanging(object sender, EventArgs e)
        {
        }

        private void RuleApplicationService_ValidationFailed(object sender, EventArgs<RuleApplicationValidationErrorCollection> e)
        {
        }
    }
}
