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

	/*	internal partial class OpenFromCatalogControl : WindowContent
	{
		public ServiceManager ServiceManager { get; set; }

		public IEnumerable RuleAppInfos { get; private set; }
		public ICommand SelectRuleAppCommand { get; private set; }
		public ICommand SearchCommand { get; private set; }
		public IVisualCommand OpenCommand { get; private set; }
		public IVisualCommand CancelCommand { get; private set; }
		public DelegateCommand ViewLabelsCommand { get; private set; }
		
		public RuleAppInfo SelectedRuleAppInfo { get; private set; }
		public string WorkspaceText { get; private set; }
	    public string ConnectionDisplayName { get; private set; }

        public string ServiceUri { get; private set; }
		public bool HasRuleAppWithWorkspace { get; private set; }

		private RuleAppInfoWrapper _selectedRuleAppInfoWrapper;
		private readonly RuleCatalogConnection _conn;

		public OpenFromCatalogControl(RuleCatalogConnection conn, IEnumerable<RuleAppInfo> ruleAppInfos, SettingsStorageService settingsStorageService)
		{
			InitializeComponent();

			SelectRuleAppCommand = new DelegateCommand(SelectRuleApp);
			SearchCommand = new DelegateCommand(Search);
			ViewLabelsCommand = new DelegateCommand(ViewLabels, false);
			OpenCommand = new VisualDelegateCommand(delegate { OnPerformButtonClick("Open"); }, null, false);
			CancelCommand = new VisualDelegateCommand(delegate { OnPerformButtonClick(Strings.Cancel); });

			WorkspaceText = string.Format("A saved version exists in the '{0}' workspace.", conn.User.Name);
			RuleAppInfos = ruleAppInfos.Convert(info => new RuleAppInfoWrapper(info));
			HasRuleAppWithWorkspace = ruleAppInfos.Any(ruleAppInfo => ruleAppInfo.WorkspaceRevisionId > 0);

			ServiceUri = conn.ServiceUri;

			var catalogName = conn.GetCatalogName();
			ConnectionDisplayName = catalogName == null ? conn.ServiceUri : $"{catalogName} [{conn.ServiceUri}]";

			_conn = conn;

			DataContext = this;
		}

		private void ViewLabels(object obj)
		{
			var controller = new RuleCatalogController(_conn, RuleCatalogControllerMode.irAuthor);

			var control = new LabelInformationControl(controller, SelectedRuleAppInfo, 0);
			var window = WindowFactory.CreateWindow("Label Information for " + SelectedRuleAppInfo.Name, control, 500, 400, true, Strings.Close);
			((Window)window).Icon = ImageFactory.GetImageAuthoringAssembly("/Images/Labels16.png");
			window.ButtonClicked += delegate
			{
				window.Close();
			};
			window.Show();
		}

		private void Search(object obj)
		{
			var selected = false;

			var viewModel = new CatalogSearchViewModel(_conn, new OpenRuleAppSearchContext());
			var control = ServiceManager.Compose<CatalogSearchControl>(viewModel);
            control.RuleApplicationService = ServiceManager.GetService<RuleApplicationService>();
            var window = WindowFactory.CreateWindow("Catalog Search" + viewModel.Text, control, 600, 450, true);
            window.ButtonClicked += delegate(object sender, WindowButtonClickedEventArgs<CatalogSearchControl> e)
            {
				if (e.ClickedButtonText == Strings.Open)
				{
					var selectedItem = viewModel.SelectedItem;

					if (selectedItem != null)
					{
                        selected = true;
					}
				}

                window.Close();
            };

            window.Show();

			if (selected && viewModel.SelectedDefInfo.Key?.Key.Guid != null )
			{
			    var ruleAppInfos = RuleAppInfos.AsGeneric<RuleAppInfoWrapper>();
                var selectedWapper = ruleAppInfos.FirstOrDefault(x => x.RuleAppInfo.AppGuid == viewModel.SelectedDefInfo.Key.Key.Guid);
			    if (selectedWapper != null)
			    {
			        SelectedRuleAppInfoWrapper = selectedWapper;
			    }
                OnPerformButtonClick(Strings.Open);
			}
        }

		private void SelectRuleApp(object obj)
		{
			OnPerformButtonClick(Strings.Open);
		}

		public RuleAppInfoWrapper SelectedRuleAppInfoWrapper
		{
			get { return _selectedRuleAppInfoWrapper; }
			set 
			{ 
				_selectedRuleAppInfoWrapper = value;

				SelectedRuleAppInfo = value == null ? null : _selectedRuleAppInfoWrapper.RuleAppInfo;

				var hasSelection = value != null;

				OpenCommand.IsEnabled = hasSelection;
				ViewLabelsCommand.IsEnabled = hasSelection;

				OnPropertyChanged(@"SelectedRuleAppInfoWrapper");				
			}
		}
	}*/
}
