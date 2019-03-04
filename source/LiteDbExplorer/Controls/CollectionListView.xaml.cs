﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
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

        private GridViewColumnHeader _lastHeaderClicked;
        private ListSortDirection _lastDirection;
        private bool _stopDoubleClick;

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
                {
                    return ListCollectionData.SelectedItems.Cast<DocumentReference>();
                }

                return null;
            }
        }

        private int DbItemsSelectedCount
        {
            get
            {
                if (ListCollectionData?.ItemsSource != null)
                {
                    return ListCollectionData.SelectedItems.Count;
                }

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
            if (!(d is CollectionListView collectionListView))
            {
                return;
            }

            if (collectionListView._modelHandled)
            {
                return;
            }

            collectionListView._modelHandled = true;
            collectionListView.SelectItems();
            collectionListView._modelHandled = false;
        }

        public void ScrollIntoItem(object item)
        {
            ListCollectionData.ScrollIntoView(item);
        }

        public void ScrollIntoSelectedItem()
        {
            ListCollectionData.ScrollIntoView(ListCollectionData.SelectedItem);
        }
        
        public void UpdateGridColumns(BsonDocument dbItem)
        {
            var headers = GridCollectionData.Columns.Select(a => (a.Header as GridViewColumnHeader)?.Name).ToArray();
            var addKeys = dbItem.Keys.Except(headers);
            var removeKeys = headers.Except(dbItem.Keys);
            
            foreach (var key in removeKeys)
            {
                RemoveGridColumn(key);
            }

            foreach (var key in addKeys)
            {
                AddGridColumn(key);
            }
        }

        public void UpdateGridColumns(CollectionReference collectionReference)
        {
            if (ListCollectionData.Items is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= OnListViewItemsChanged;
            }

            ListCollectionData.ItemsSource = null;

            GridCollectionData.Columns.Clear();

            if (collectionReference == null)
            {
                return;
            }

            ListCollectionData.ItemsSource = collectionReference.Items;
            
            var keys = new List<string>();
            foreach (var item in collectionReference.Items)
            {
                keys = item.LiteDocument.Keys.Union(keys).ToList();
            }

            if (App.Settings.FieldSortOrder == FieldSortOrder.Alphabetical)
            {
                keys = keys.OrderBy(a => a).ToList();
            }

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
            if (DoubleClickCommand?.CanExecute(SelectedItem) != true || _stopDoubleClick)
            {
                return;
            }

            DoubleClickCommand?.Execute(SelectedItem);
        }

        private void OnListViewItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_viewHandled)
            {
                return;
            }

            if (ListCollectionData.Items.SourceCollection == null)
            {
                return;
            }

            SelectItems();
        }

        private void OnListViewSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_viewHandled)
            {
                return;
            }

            if (ListCollectionData.Items.SourceCollection == null)
            {
                return;
            }

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
                Header = new GridViewColumnHeader
                {
                    Content = key,
                    Tag = key,
                    Name = key,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch
                },
                DisplayMemberBinding = new Binding
                {
                    Path = new PropertyPath($"LiteDocument[{key}]"),
                    Mode = BindingMode.OneWay,
                    Converter = new BsonValueToStringConverter()
                },
                HeaderTemplate = Resources["HeaderTemplate"] as DataTemplate
            });
        }

        private void RemoveGridColumn(string key)
        {
            GridViewColumn columnToRemove = null;
            foreach (var gridViewColumn in GridCollectionData.Columns)
            {
                if (gridViewColumn.Header is GridViewColumnHeader header && header.Tag.Equals(key))
                {
                    columnToRemove = gridViewColumn;
                }
            }

            if (columnToRemove != null)
            {
                GridCollectionData.Columns.Remove(columnToRemove);
            } 
        }

        private void ListCollectionData_OnHeaderClick(object sender, RoutedEventArgs e)
        {
            _stopDoubleClick = true;

            if (e.OriginalSource is GridViewColumnHeader headerClicked)
            {
                if (headerClicked.Role != GridViewColumnHeaderRole.Padding)
                {
                    ListSortDirection direction;

                    if (headerClicked != _lastHeaderClicked)
                    {
                        direction = ListSortDirection.Ascending;
                    }
                    else
                    {
                        direction = _lastDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                    }

                    var sortBy = headerClicked.Name;

                    Sort(sortBy, direction);

                    if (direction == ListSortDirection.Ascending)
                    {
                        headerClicked.Column.HeaderTemplate =
                            Resources["HeaderTemplateArrowUp"] as DataTemplate;
                    }
                    else
                    {
                        headerClicked.Column.HeaderTemplate =
                            Resources["HeaderTemplateArrowDown"] as DataTemplate;
                    }

                    // Remove arrow from previously sorted header  
                    if (_lastHeaderClicked != null && _lastHeaderClicked != headerClicked)
                    {
                        _lastHeaderClicked.Column.HeaderTemplate = Resources["HeaderTemplate"] as DataTemplate;
                    }

                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }

            _stopDoubleClick = false;
        }

        // Sort code
        private void Sort(string sortBy, ListSortDirection direction)  
        {  
            var dataView =  (ListCollectionView)CollectionViewSource.GetDefaultView(ListCollectionData.ItemsSource);
            dataView.CustomSort = new SortBsonValue(sortBy, direction == ListSortDirection.Descending);
        }
        
        public class SortBsonValue : IComparer
        {
            private readonly string _key;
            private readonly bool _reverse;

            public SortBsonValue(string key, bool reverse)
            {
                _key = key;
                _reverse = reverse;
            }

            public int Compare(object x, object y)
            {
                var doc1 = x as DocumentReference;
                var doc2 = y as DocumentReference;

                if(doc1 == null && doc2 == null)
                {
                    return 0;
                }

                if(doc1 == null)
                {
                    return _reverse ? 1 : -1;
                }

                if(doc2 == null)
                {
                    return _reverse ? -1 : 1;
                }

                var bsonValue1 = doc1.LiteDocument[_key];
                var bsonValue2 = doc2.LiteDocument[_key];

                if (_reverse)
                {
                    return bsonValue2.CompareTo(bsonValue1);
                }

                return bsonValue1.CompareTo(bsonValue2);
            }
        }
        
    }
}