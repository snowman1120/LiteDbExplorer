using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using LiteDbExplorer.Presentation.Converters;
using LiteDB;

namespace LiteDbExplorer.Controls
{
    /// <summary>
    ///     Interaction logic for CollectionListView.xaml
    /// </summary>
    public partial class CollectionListView : UserControl
    {
        public static readonly DependencyProperty CollectionReferenceProperty = DependencyProperty.Register(
            nameof(CollectionReference),
            typeof(CollectionReference),
            typeof(CollectionListView),
            new PropertyMetadata(null, OnCollectionReferenceChanged));

        public static readonly DependencyProperty SelectedItemProperty = DependencyProperty.Register(
            nameof(SelectedItem),
            typeof(object),
            typeof(CollectionListView),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty SelectedItemsProperty = DependencyProperty.Register(
            nameof(SelectedItems),
            typeof(IList),
            typeof(CollectionListView),
            new FrameworkPropertyMetadata(default(IList), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedItemsChanged));

        public static readonly DependencyProperty DoubleClickCommandProperty = DependencyProperty.Register(
            nameof(DoubleClickCommand), typeof(ICommand), typeof(CollectionListView),
            new PropertyMetadata(default(ICommand)));

        private bool _modelHandled;
        private bool _viewHandled;

        public CollectionListView()
        {
            InitializeComponent();
            
            ListCollectionData.MouseDoubleClick += ListCollectionDataOnMouseDoubleClick;
            ListCollectionData.SelectionChanged += OnListViewSelectionChanged;

            ListCollectionData.SetBinding(Selector.SelectedItemProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath(nameof(SelectedItem)),
                Mode = BindingMode.TwoWay
            });
        }

        public CollectionReference CollectionReference
        {
            get => (CollectionReference) GetValue(CollectionReferenceProperty);
            set => SetValue(CollectionReferenceProperty, value);
        }

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }

        public IList SelectedItems
        {
            get => (IList) GetValue(SelectedItemsProperty);
            set => SetValue(SelectedItemsProperty, value);
        }

        public ICommand DoubleClickCommand
        {
            get => (ICommand) GetValue(DoubleClickCommandProperty);
            set => SetValue(DoubleClickCommandProperty, value);
        }

        public ContextMenu ListViewContextMenu
        {
            get => ListCollectionData.ContextMenu;
            set => ListCollectionData.ContextMenu = value;
        }

        private IEnumerable<DocumentReference> DbSelectedItems
        {
            get
            {
                if (ListCollectionData.Visibility == Visibility.Visible)
                    return ListCollectionData.SelectedItems.Cast<DocumentReference>();

                return null;
            }
        }

        private int DbItemsSelectedCount
        {
            get
            {
                if (ListCollectionData?.ItemsSource != null) return ListCollectionData.SelectedItems.Count;

                return 0;
            }
        }

        private static void OnCollectionReferenceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var collectionListView = d as CollectionListView;
            var collectionReference = e.NewValue as CollectionReference;
            collectionListView?.UpdateGridColumns(collectionReference);
        }

        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var collectionListView = d as CollectionListView;

            if (collectionListView._modelHandled)
                return;

            collectionListView._modelHandled = true;
            collectionListView.SelectItems();
            collectionListView._modelHandled = false;
        }

        public void ScrollIntoSelectedItem()
        {
            ListCollectionData.ScrollIntoView(ListCollectionData.SelectedItem);
        }

        public void ScrollIntoView(object item)
        {
            ListCollectionData.ScrollIntoView(item);
        }

        public void UpdateGridColumns(BsonDocument dbItem)
        {
            var headers = GridCollectionData.Columns.Select(a => (a.Header as TextBlock)?.Text);
            var missing = dbItem.Keys.Except(headers);

            foreach (var key in missing) AddGridColumn(key);
        }

        public void UpdateGridColumns(CollectionReference collectionReference)
        {
            if (ListCollectionData.Items is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= OnListViewItemsChanged;
            }

            ListCollectionData.ItemsSource = null;

            GridCollectionData.Columns.Clear();

            if (collectionReference == null) return;

            ListCollectionData.ItemsSource = collectionReference.Items;
            
            var keys = new List<string>();
            foreach (var item in collectionReference.Items) keys = item.LiteDocument.Keys.Union(keys).ToList();

            if (App.Settings.FieldSortOrder == FieldSortOrder.Alphabetical) keys = keys.OrderBy(a => a).ToList();

            foreach (var key in keys)
            {
                AddGridColumn(key);
            }

            if (ListCollectionData.Items is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += OnListViewItemsChanged;
            }
        }
        
        private void ListCollectionDataOnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DoubleClickCommand?.CanExecute(SelectedItem) != true) return;

            DoubleClickCommand?.Execute(SelectedItem);
        }

        private void OnListViewItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_viewHandled) return;
            if (ListCollectionData.Items.SourceCollection == null) return;

            SelectItems();
        }
        
        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewHandled) return;
            if (ListCollectionData.Items.SourceCollection == null) return;

            SelectedItems = ListCollectionData.SelectedItems.Cast<DocumentReference>().ToList();
        }
        
        private void SelectItems()
        {
            _viewHandled = true;
            /*SelectedItems.Clear();
            if (SelectedItems != null)
            {
                foreach (var item in SelectedItems)
                {
                    SelectedItems.Add(item);
                }
            }*/
            _viewHandled = false;
        }

        private void AddGridColumn(string key)
        {
            GridCollectionData.Columns.Add(new GridViewColumn
            {
                Header = new TextBlock {Text = key},
                DisplayMemberBinding = new Binding
                {
                    Path = new PropertyPath($"LiteDocument[{key}]"),
                    Mode = BindingMode.OneWay,
                    Converter = new BsonValueToStringConverter()
                }
            });
        }
    }
}