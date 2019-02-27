using System.Windows;
using System.Windows.Controls;

namespace LiteDbExplorer.Modules.Main
{
    /// <summary>
    /// Interaction logic for ShellRightMenuView.xaml
    /// </summary>
    public partial class ShellRightMenuView : UserControl
    {
        public ShellRightMenuView()
        {
            InitializeComponent();
        }

        private void UpdatePanelButtonOnClick(object sender, RoutedEventArgs e)
        {
            DropNewUpdatePanel.IsPopupOpen = true;
        }
    }
}
