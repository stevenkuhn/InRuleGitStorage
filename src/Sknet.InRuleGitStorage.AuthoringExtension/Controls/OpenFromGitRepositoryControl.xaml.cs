using InRule.Authoring;
using InRule.Authoring.Commanding;
using InRule.Authoring.Controls;
using InRule.Authoring.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Sknet.InRuleGitStorage.AuthoringExtension.Controls
{
    /// <summary>
    /// Interaction logic for OpenFromGitRepositoryControl.xaml
    /// </summary>
    public partial class OpenFromGitRepositoryControl : WindowContent
	{
		public string ConnectionDisplayName { get; private set; }
		public IEnumerable<RuleApplicationGitInfo> RuleAppInfos { get; private set; }
		public ICommand SelectRuleAppCommand { get; private set; }
		public IVisualCommand OpenCommand { get; private set; }
		public IVisualCommand CancelCommand { get; private set; }

		public OpenFromGitRepositoryControl(GitRepositoryOption selectedOption, IEnumerable<RuleApplicationGitInfo> ruleAppInfos)
		{
			InitializeComponent();

			var repositoryName = selectedOption.Name;
			ConnectionDisplayName = string.IsNullOrWhiteSpace(repositoryName) ? selectedOption.SourceUrl : $"{repositoryName} [{selectedOption.SourceUrl}]";

			RuleAppInfos = ruleAppInfos;

			SelectRuleAppCommand = new DelegateCommand(SelectRuleApp);
			OpenCommand = new VisualDelegateCommand(delegate { OnPerformButtonClick("Open"); }, null, false);
			CancelCommand = new VisualDelegateCommand(delegate { OnPerformButtonClick(Strings.Cancel); });

			DataContext = this;
		}

		private void SelectRuleApp(object obj)
		{
			OnPerformButtonClick(Strings.Open);
		}

		private RuleApplicationGitInfo _selectedRuleApplicationGitInfo;
		public RuleApplicationGitInfo SelectedRuleApplicationGitInfo
		{
			get { return _selectedRuleApplicationGitInfo; }
			set
			{
				_selectedRuleApplicationGitInfo = value;

				//SelectedRuleAppInfo = value == null ? null : _selectedRuleAppInfoWrapper.RuleAppInfo;

				var hasSelection = value != null;

				OpenCommand.IsEnabled = hasSelection;
				//ViewLabelsCommand.IsEnabled = hasSelection;

				OnPropertyChanged(@"SelectedRuleApplicationGitInfo");
			}
		}
	}
}
