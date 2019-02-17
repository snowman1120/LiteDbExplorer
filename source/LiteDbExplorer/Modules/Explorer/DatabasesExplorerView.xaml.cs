using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LiteDbExplorer.Presentation;

namespace LiteDbExplorer.Modules.Explorer
{
    /// <summary>
    /// Interaction logic for DatabasesNavView.xaml
    /// </summary>
    public partial class DatabasesExplorerView : UserControl, IFileDropSource
    {
        public DatabasesExplorerView()
        {
            InitializeComponent();
        }

        public Action<IEnumerable<string>> FilesDropped { get; set; }

        private void DockPanel_OnDrop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop) && e.Data.GetData(DataFormats.FileDrop, false) is string[] files)
                {
                    FilesDropped?.Invoke(files);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show("Failed to open database: " + exc.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
                treeViewItem.IsSelected = true;
                e.Handled = true;
            }
        }
    }
}
