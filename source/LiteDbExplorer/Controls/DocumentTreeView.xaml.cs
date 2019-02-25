using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using LiteDB;

namespace LiteDbExplorer.Controls
{
    /// <summary>
    /// Interaction logic for DocumentTreeView.xaml
    /// </summary>
    public partial class DocumentTreeView : UserControl
    {
        public DocumentTreeView()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty DocumentSourceProperty = DependencyProperty.Register(
            nameof(DocumentSource), 
            typeof(DocumentReference), 
            typeof(DocumentTreeView), 
            new PropertyMetadata(null, propertyChangedCallback: OnDocumentSourceChanged));

        private static void OnDocumentSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DocumentTreeView documentTreeView))
            {
                return;
            }

            if (e.NewValue is DocumentReference documentReference)
            {
                documentTreeView.ItemsSource = new DocumentTreeItemsSource(documentReference);
            }
            else
            {
                documentTreeView.ItemsSource = null;
            }
        }

        public DocumentReference DocumentSource
        {
            get => (DocumentReference) GetValue(DocumentSourceProperty);
            set => SetValue(DocumentSourceProperty, value);
        }

        public IEnumerable ItemsSource
        {
            get => DocumentTree.ItemsSource;
            set => DocumentTree.ItemsSource = value;
        }

        public void UpdateDocument()
        {
            ItemsSource = DocumentSource != null ? new DocumentTreeItemsSource(DocumentSource) : null;
        }

        public void InvalidateItemsSource(object item)
        {
            if (ItemsSource is DocumentTreeItemsSource treeItems && item is BsonDocument document)
            {
                treeItems.Invalidate(document);
            }
            else
            {
                DocumentTree.InvalidateProperty(ItemsControl.ItemsSourceProperty);
            }
        }

        private void OnCurrentThemeChanged(object sender, EventArgs e)
        {
            InvalidateItemsSource(null);
        }
    }

    public class DocumentTreeItemsSource : IEnumerable<DocumentFieldNode>, INotifyPropertyChanged
    {
        public DocumentTreeItemsSource(DocumentReference document)
        {
            InstanceId = document.InstanceId;
            Nodes = GetNodes(document.LiteDocument);
        }

        public string InstanceId { get; }

        public ObservableCollection<DocumentFieldNode> Nodes { get; set; }

        public ObservableCollection<DocumentFieldNode> GetNodes(BsonDocument document)
        {
            var nodes = new ObservableCollection<DocumentFieldNode>();
            for (var i = 0; i < document.Keys.Count; i++)
            {
                var key = document.Keys.ElementAt(i);
                var bsonValue = document[key];

                Func<BsonDocument, ObservableCollection<DocumentFieldNode>> loadAction = null;
                
                if (bsonValue != null && (bsonValue.IsArray || bsonValue.IsDocument))
                {
                    loadAction = GetNodes;
                }

                var fieldNode = new DocumentFieldNode(key, bsonValue, loadAction);

                nodes.Add(fieldNode);
            }

            return nodes;
        }

        public void Invalidate(BsonDocument document)
        {
            Nodes = GetNodes(document);
        }

        public IEnumerator<DocumentFieldNode> GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) Nodes).GetEnumerator();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [JetBrains.Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DocumentFieldNode : INotifyPropertyChanged
    {
        private bool _isExpanded;
#pragma warning disable CS0414 // The field 'DocumentFieldNode._loaded' is assigned but its value is never used
        private bool _loaded;
#pragma warning restore CS0414 // The field 'DocumentFieldNode._loaded' is assigned but its value is never used

        private readonly Func<BsonDocument, ObservableCollection<DocumentFieldNode>> _loadNodes;

        private DocumentFieldNode()
        {
        }
        
        public DocumentFieldNode(string key, BsonValue value, Func<BsonDocument, ObservableCollection<DocumentFieldNode>> loadNodes)
        {
            _loadNodes = loadNodes;

            Initialize(key, value);
        }

        public int? NodesCount { get; set; }

        public string NodesCountText { get; set; }

        public string Key { get; set; }
        
        public BsonValue Value { get; set; }

        public bool IsSelected { get; set; }

        public BsonType? ValueType { get; set; }
        
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                OnNodeExpanded();
            }
        }
        
        public ObservableCollection<DocumentFieldNode> Nodes { get; set; }
        
        protected void Initialize(string key, BsonValue value)
        {
            Key = key;
            Value = value;

            // TODO: Infer Null value type to handle
            if (Value != null)
            {
                ValueType = Value.Type;
            }

            if (value is BsonDocument document)
            {
                NodesCount = document.Count;
            }

            if (value is BsonArray array)
            {
                NodesCount = array.Count;
            }

            if (NodesCount.HasValue)
            {
                var suffix = NodesCount == 1 ? "Item" : "Items";
                NodesCountText = $" {NodesCount} {suffix}";
            }
            
            if (_loadNodes != null)
            {
                // Add Dummy load node
                Nodes = new ObservableCollection<DocumentFieldNode>
                {
                    new DocumentFieldNode()
                };
            }
        }

        private void OnNodeExpanded()
        {
            if(IsExpanded == false)
            {
                return;
            }

            if (_loadNodes != null && Value is BsonDocument document)
            {
                _loaded = true;
                Nodes = _loadNodes(document);
            }

            if (_loadNodes != null && Value is BsonArray array)
            {
                _loaded = true;
                var index = 0;
                var nodes = new ObservableCollection<DocumentFieldNode>();
                foreach (var arrayDoc in array)
                {
                    if (arrayDoc is BsonDocument || arrayDoc is BsonArray)
                    {
                        nodes.Add(new DocumentFieldNode(index.ToString(), arrayDoc, _loadNodes));
                    }
                    else
                    {
                        nodes.Add(new DocumentFieldNode(index.ToString(), arrayDoc, null));
                    }
                    
                    index++;
                }

                Nodes = nodes;
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [JetBrains.Annotations.NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
