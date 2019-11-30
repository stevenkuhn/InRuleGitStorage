using InRule.Authoring.Controls;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

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
