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

namespace LiteDbExplorer.Wpf.Modules.Settings
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView : Window
    {
        public SettingsView()
        {
            InitializeComponent();

            ContentItemsControl.ScrollIntoView();
        }
    }

    public static class ItemsControlScrollExtensions
    {
        public static void ScrollIntoView(this ItemsControl control, object item)
        {
            FrameworkElement framework = control.ItemContainerGenerator.ContainerFromItem(item) as FrameworkElement;
            framework?.BringIntoView();
        }

        public static void ScrollIntoView(this ItemsControl control)
        {
            var count = control.Items.Count;
            if (count == 0)
            {
                return;
            }
            var item = control.Items[count - 1];
            control.ScrollIntoView(item);
        }
    }
}
