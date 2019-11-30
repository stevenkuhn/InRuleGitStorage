﻿using InRule.Authoring;
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
            //if (filename != "git://")
            //{
            //    return base.OpenFromFilename(filename);
            //}

            //var selectedOption = GetSelectedGitRepositoryOption();

            //if (selectedOption == null)
            //{
            //    return false;
            //}

            //RuleApplicationGitInfo[] ruleApplications = null;

            //var waitWindow = new BackgroundWorkerWaitWindow("Open from Git Repository", "Retrieving rule applications...");
            //waitWindow.DoWork += delegate
            //{
            //    using (var repository = InRuleGitRepository.Open(selectedOption.WorkingDirectory))
            //    {
            //        ruleApplications = repository.GetRuleApplications();
            //    }
            //};

            //waitWindow.RunWorkerCompleted += delegate (object sender, RunWorkerCompletedEventArgs e)
            //{
            //    if (e.Error != null)
            //    {
            //        throw new NotImplementedException();
            //    }
            //};

            //waitWindow.ShowDialog();

            //if (ruleApplications == null)
            //{
            //    throw new NotImplementedException();
            //}

            //var control = ServiceManager.Compose<>();
            //var window = WindowFactory.CreateWindow("title", control, width, height, true);

            /*if (ruleAppInfos != null)
            {
                RuleAppInfo selectedRuleAppInfo = null;

                var control = ServiceManager.Compose<OpenFromCatalogControl>(conn, ruleAppInfos, SettingsStorageService);
                var window = WindowFactory.CreateWindow("Open from Catalog", control, 800, 350, true);
                window.ButtonClicked += delegate (object sender, WindowButtonClickedEventArgs<OpenFromCatalogControl> e)
                {
                    if (e.ClickedButtonText == Strings.Open)
                    {
                        selectedRuleAppInfo = control.SelectedRuleAppInfo;
                    }

                    window.Close();
                };

                window.Show();

                if (selectedRuleAppInfo != null)
                {
                    success = OpenRuleApp(selectedRuleAppInfo.AppGuid, selectedRuleAppInfo.Name, selectedRuleAppInfo.CheckedOutBy, conn);
                }
            }*/

            throw new NotImplementedException();

            /*var success = true;
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

            return success;*/
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

                if (!InRuleGitRepository.IsValid(selectedOption.WorkingDirectory))
                {
                    throw new NotImplementedException();
                }

                using (var repository = InRuleGitRepository.Open(selectedOption.WorkingDirectory))
                {
                    var signature = repository.Config.BuildSignature(DateTimeOffset.UtcNow);

                    repository.Commit(ruleAppDef, $"Saving rule app {ruleAppDef.Name}", signature, signature);
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
                }

                var newPersistenceInfo = new GitPersistenceInfo(ruleAppDef.Name, selectedOption.WorkingDirectory, selectedOption);

                if (!_ruleApplicationService.PersistenceInfo.IsSame(newPersistenceInfo))
                {
                    _ruleApplicationService.SetRuleApplicationDef(ruleAppDef, newPersistenceInfo);
                }

                success = true;
            }
            catch (Exception e)
            {
                var text = string.Format(Strings.The_following_error_occurred___0, e.Message);
                MessageBoxFactory.Show(text, "Error saving rule application", MessageBoxFactoryImage.Error);
            }

            return success;

            /*var success = false;
            

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
                }/

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

            return success;*/
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
