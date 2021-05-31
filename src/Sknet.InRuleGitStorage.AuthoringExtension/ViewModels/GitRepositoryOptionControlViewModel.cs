using InRule.Authoring.Commanding;
using InRule.Authoring.ViewModels;
using InRule.Common.Utilities;
using Sknet.InRuleGitStorage.AuthoringExtension.Controls;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace Sknet.InRuleGitStorage.AuthoringExtension.ViewModels
{
    public class GitRepositoryOptionControlViewModel : ViewModelBase
    {
        public event EventHandler UseThisClicked;

        public DelegateCommand AddGitRepositoryOptionCommand { get; }
        public ObservableCollection<GitRepositoryOptionViewModel> GitRepositoryOptions { get; }
        public GitRepositoryOptionControl View { get; set; }
        public Window OwningWindow { get; set; }
        public GitRepositorySettings Settings { get; private set; }

        public GitRepositoryOptionControlViewModel()
        {
            GitRepositoryOptions = new ObservableCollection<GitRepositoryOptionViewModel>();

            AddGitRepositoryOptionCommand = new DelegateCommand(AddGitRepositoryOption);
        }

        protected void AddGitRepositoryOption(object obj)
        {
            GitRepositoryOptions.Insert(0, new GitRepositoryOptionViewModel(this, new GitRepositoryOption()) { IsExpanded = true });
        }

        protected void RaiseUseThisClicked(EventArgs<GitRepositoryOptionViewModel> eventArgs)
        {
            UseThisClicked?.Invoke(this, eventArgs);
        }

        public void LoadSettings()
        {
            Settings = GitRepositorySettings.Load(SettingsStorageService);

            GitRepositoryOptions.Clear();

            foreach (var option in Settings.Options)
            {
                GitRepositoryOptions.Add(new GitRepositoryOptionViewModel(this, option));
            }

            if (GitRepositoryOptions.Count == 0)
            {
                var viewModel = new GitRepositoryOptionViewModel(this, new GitRepositoryOption());
                GitRepositoryOptions.Add(viewModel);
            }

            if (GitRepositoryOptions.Count == 1)
            {
                GitRepositoryOptions[0].IsExpanded = true;
            }
        }

        public void SaveSettings()
        {
            Settings.Options.Clear();

            foreach (var viewModel in GitRepositoryOptions)
            {
                Settings.Options.Add(viewModel.Model);
            }

            Settings.Save(SettingsStorageService);
        }

        public void UseThisGitRepository(GitRepositoryOptionViewModel viewModel)
        {
            SaveSettings();
            RaiseUseThisClicked(new EventArgs<GitRepositoryOptionViewModel>(viewModel));
        }
    }
}
