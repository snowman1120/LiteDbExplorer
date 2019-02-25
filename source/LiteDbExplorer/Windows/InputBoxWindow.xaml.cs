using System.Windows;
using MahApps.Metro.Controls;

namespace LiteDbExplorer.Windows
{
    /// <summary>
    /// Interaction logic for InputBoxWindow.xaml
    /// </summary>
    public partial class InputBoxWindow : MetroWindow
    {        
        public string Text => TextText.Text;

        public InputBoxWindow()
        {
            InitializeComponent();
        }

        public static bool? ShowDialog(string message, string caption, string predefined,
            out string input)
        {
            return ShowDialog(message, caption, predefined, null, out input);
        }

        public static bool? ShowDialog(string message, string caption, string predefined, Window owner, out string input)
        {
            var window = new InputBoxWindow
            {
                Owner = owner,
                TextMessage = {Text = message}, Title = caption, TextText = {Text = predefined}
            };

            var result = window.ShowDialog();
            input = window.Text;
            return result;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TextText.Focus();
            TextText.SelectAll();
        }
    }
}
