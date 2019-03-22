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

        public static TreeViewItem ContainerFromItemRecursive(this ItemContainerGenerator root, object item)
        {
            if (root.ContainerFromItem(item) is TreeViewItem treeViewItem)
            {
                return treeViewItem;
            }

            foreach (var subItem in root.Items)
            {
                treeViewItem = root.ContainerFromItem(subItem) as TreeViewItem;
                var search = treeViewItem?.ItemContainerGenerator.ContainerFromItemRecursive(item);
                if (search != null)
                {
                    return search;
                }
            }
            return null;
        }
    }
}