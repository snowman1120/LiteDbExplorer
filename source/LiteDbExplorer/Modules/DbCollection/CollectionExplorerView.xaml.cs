using System.Windows.Controls;

namespace LiteDbExplorer.Modules.DbCollection
{
    /// <summary>
    /// Interaction logic for CollectionExplorerView.xaml
    /// </summary>
    public partial class CollectionExplorerView : UserControl, ICollectionReferenceListView
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
