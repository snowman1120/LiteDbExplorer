using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LiteDbExplorer.Presentation
{
    public static class VisualTreeExtensions
    {
        public static TreeViewItem VisualUpwardSearch(this DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }

            return source as TreeViewItem;
        }
    }
}