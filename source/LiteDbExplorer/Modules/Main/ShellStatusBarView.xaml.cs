using System.Windows.Controls;
using System.Windows.Input;

namespace LiteDbExplorer.Modules.Main
{
    /// <summary>
    /// Interaction logic for ShellStatusBarView.xaml
    /// </summary>
    public partial class ShellStatusBarView : UserControl
    {
        public ShellStatusBarView()
        {
            InitializeComponent();
        }

        private void ChangeThemeLabel_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ChangeThemeComboBox.IsDropDownOpen = !ChangeThemeComboBox.IsDropDownOpen;
        }
    }
}
