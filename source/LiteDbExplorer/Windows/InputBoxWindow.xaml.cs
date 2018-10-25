using System.Windows;
using MahApps.Metro.Controls;

namespace LiteDbExplorer.Windows
{
    /// <summary>
    /// Interaction logic for InputBoxWindow.xaml
    /// </summary>
    public partial class InputBoxWindow : MetroWindow
    {        
        public string Text
        {
            get
            {
                return TextText.Text;
            }
        }

        public InputBoxWindow()
        {
            InitializeComponent();
        }

        public static bool? ShowDialog(string message, string caption, string predefined, out string input)
        {
            var window = new InputBoxWindow();
            window.TextMessage.Text = message;
            window.Title = caption;
            window.TextText.Text = predefined;

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
