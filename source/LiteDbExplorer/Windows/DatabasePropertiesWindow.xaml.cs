using LiteDB;
using System.Windows;
using System.Windows.Data;
using MahApps.Metro.Controls;
using Xceed.Wpf.Toolkit;

namespace LiteDbExplorer.Windows
{
    /// <summary>
    /// Interaction logic for DatabasePropertiesWindow.xaml
    /// </summary>
    public partial class DatabasePropertiesWindow : MetroWindow
    {
        public LiteDatabase Database
        {
            get; set;
        }

        public DatabasePropertiesWindow(LiteDatabase database)
        {
            InitializeComponent();
            Database = database;
            InputVersion.DataContext = Database.Engine;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            var binding = BindingOperations.GetBindingExpression(InputVersion, IntegerUpDown.ValueProperty);
            if (binding.IsDirty)
            {
                binding.UpdateSource();
            }

            DialogResult = true;
            Close();
        }

        private void ButtonShrink_Click(object sender, RoutedEventArgs e)
        {
            Database.Shrink();
        }

        private void ButtonPassword_Click(object sender, RoutedEventArgs e)
        {
            if (InputBoxWindow.ShowDialog("New password, enter empty string to remove password.", "", "", out string password) == true)
            {
                Database.Shrink(string.IsNullOrEmpty(password) ? null : password);
            }
        }
    }
}
