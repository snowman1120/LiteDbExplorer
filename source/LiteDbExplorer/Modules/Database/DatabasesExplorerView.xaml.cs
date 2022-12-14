using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LiteDbExplorer.Framework.Services;
using LiteDbExplorer.Presentation;

namespace LiteDbExplorer.Modules.Database
{
    /// <summary>
    /// Interaction logic for DatabasesNavView.xaml
    /// </summary>
    public partial class DatabasesExplorerView : UserControl
    {
        public DatabasesExplorerView()
        {
            InitializeComponent();

            TreeDatabase.PreviewMouseRightButtonDown += (sender, e) =>
            {
                if ((e.OriginalSource as DependencyObject).VisualUpwardSearch() != null)
                {
                    return;
                }

                if (sender is TreeView treeView && treeView.SelectedItem != null)
                {
                    if (treeView.ItemContainerGenerator.ContainerFromItemRecursive(treeView.SelectedItem) is TreeViewItem treeViewItem)
                    {
                        treeViewItem.Focus();
                        treeViewItem.IsSelected = true;
                        e.Handled = true;
                    }
                }
            };
        }
        
        private void RecentItemMoreBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (OpenDatabase.ContextMenu != null)
            {
                OpenDatabase.ContextMenu.IsEnabled = true;
                OpenDatabase.ContextMenu.PlacementTarget = OpenDatabase;
                OpenDatabase.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                OpenDatabase.ContextMenu.IsOpen = true;
            }
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = (e.OriginalSource as DependencyObject).VisualUpwardSearch();
            if (treeViewItem != null)
            {
                treeViewItem.Focus();
                treeViewItem.IsSelected = true;
                e.Handled = true;
            }
        }
    }
}
