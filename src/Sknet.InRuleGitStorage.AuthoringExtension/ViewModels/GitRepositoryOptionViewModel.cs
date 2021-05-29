using InRule.Authoring.Commanding;
using InRule.Authoring.ViewModels;
using InRule.Authoring.Windows;
using System;

namespace Sknet.InRuleGitStorage.AuthoringExtension.ViewModels
{
    public class GitRepositoryOptionViewModel : ViewModelBase
    {
        public DelegateCommand DeleteCommand { get; }
        public DelegateCommand MoveUpCommand { get; }
        public DelegateCommand MoveDownCommand { get; }
        public DelegateCommand UseThisCommand { get; }

        public GitRepositoryOptionControlViewModel Parent { get; private set; }
        public GitRepositoryOption Model { get; set; }

        public bool IsExpanded { get; set; }

        public string Name
        {
            get { return Model.Name; }

            set
            {
                if (Model.Name != value)
                {
                    Model.Name = value;
                    OnPropertyChanged(nameof(Name));
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public override string DisplayName
        {
            get
            {
                var localName = Name.Trim();
                if (!string.IsNullOrEmpty(localName))
                {
                    return localName;
                }
                if (!string.IsNullOrEmpty(SourceUrl))
                {
                    return SourceUrl;
                }
                return "(no name)";
            }
        }

        public string SourceUrl
        {
            get { return Model.SourceUrl; }

            set
            {
                if (Model.SourceUrl != value)
                {
                    Model.SourceUrl = value;
                    OnPropertyChanged(nameof(SourceUrl));
                    OnPropertyChanged(nameof(DisplayName));
                }
            }
        }

        public GitRepositoryOptionViewModel(GitRepositoryOptionControlViewModel parent, GitRepositoryOption model)
        {
            Parent = parent;
            Model = model;

            IsExpanded = false;

            DeleteCommand = new DelegateCommand(Delete);
            MoveUpCommand = new DelegateCommand(MoveUp);
            MoveDownCommand = new DelegateCommand(MoveDown);
            UseThisCommand = new DelegateCommand(UseThis);

            UpdateButtonState();
            Parent.GitRepositoryOptions.CollectionChanged += delegate { UpdateButtonState(); };
        }

        private void Delete(object obj)
        {
            //Parent.DeleteCatalog(this);
        }

        private void MoveUp(object obj)
        {
            //Parent.MoveUpCatalog(this);
        }

        private void MoveDown(object obj)
        {
            //Parent.MoveDownCatalog(this);
        }

        private void UpdateButtonState()
        {
            //var index = Parent.GitRepositoryOptions.IndexOf(this);
            //
            //MoveUpCommand.IsEnabled = myIndex > 0;
            //MoveDownCommand.IsEnabled = myIndex < Parent.Catalogs.Count - 1;
        }

        private void UseThis(object obj)
        {
            Name = Name.Trim();
            if (Validate())
            {
                Parent.UseThisGitRepository(this);
            }
        }

        private new bool Validate()
        {
            string errorText = null;
            Uri uri;

            if (!Uri.TryCreate(SourceUrl, UriKind.Absolute, out uri))
            {
                errorText = "Please enter a valid URL.";
            }

            if (errorText != null)
            {
                MessageBoxFactory.Show(errorText, "Git Repository", Parent.OwningWindow);
                return false;
            }

            return true;
        }
    }
}
