using InRule.Authoring.Controls;
using System.Windows.Input;

namespace Sknet.InRuleGitStorage.AuthoringExtension.Controls
{
    /// <summary>
    /// Interaction logic for GItRepositorySelectionControl.xaml
    /// </summary>
    public partial class GitRepositoryOptionControl : WindowContent
    {
        public GitRepositoryOptionControl()
        {
            InitializeComponent();
        }

        private void OfficeSeparator_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount != 2)
            {
                e.Handled = true;
            }
        }

        private void AdvancedOptions_OfficeSeparator_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
    }
}
