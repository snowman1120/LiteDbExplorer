using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LiteDbExplorer.Framework.Services;

namespace LiteDbExplorer.Modules.DbCollection
{
    /// <summary>
    /// Interaction logic for CollectionExplorerView.xaml
    /// </summary>
    public partial class CollectionExplorerView : UserControl, ICollectionListView
    {
        public CollectionExplorerView()
        {
            InitializeComponent();

            splitOrientationSelector.SelectionChanged += (sender, args) =>
            {
                splitContainer.Orientation = splitOrientationSelector.SelectedIndex == 0
                    ? Orientation.Vertical
                    : Orientation.Horizontal;
            };
        }

        public void ScrollIntoItem(object item)
        {
            CollectionListView.ScrollIntoItem(item);
        }

        public void ScrollIntoSelectedItem()
        {
            CollectionListView.ScrollIntoSelectedItem();
        }

        public void UpdateView(CollectionReference collectionReference)
        {
            CollectionListView.UpdateGridColumns(collectionReference);
        }

        public void UpdateView(DocumentReference documentReference)
        {
            CollectionListView.UpdateGridColumns(documentReference.LiteDocument);
        }
    }
}
