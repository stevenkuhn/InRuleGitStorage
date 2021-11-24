namespace Sknet.InRuleGitStorage.AuthoringExtension.Controls;

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
