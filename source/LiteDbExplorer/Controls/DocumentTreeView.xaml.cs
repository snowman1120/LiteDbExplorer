using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using LiteDB;
using PropertyTools.Wpf;

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
        
        public IEnumerable ItemsSource
        {
            get => DocumentTree.ItemsSource;
            set => DocumentTree.ItemsSource = value;
        }
    }

    public class DocumentTreeItemsSource : IEnumerable<DocumentFieldNode>, INotifyPropertyChanged
    {
        public DocumentTreeItemsSource(DocumentReference document)
        {
            Nodes = GetNodes(document.LiteDocument);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<DocumentFieldNode> Nodes { get; set; }

        public ObservableCollection<DocumentFieldNode> GetNodes(BsonDocument document)
        {
            var nodes = new ObservableCollection<DocumentFieldNode>();
            for (var i = 0; i < document.Keys.Count; i++)
            {
                var key = document.Keys.ElementAt(i);
                var bsonValue = document[key];

                Func<BsonDocument, ObservableCollection<DocumentFieldNode>> loadAction = null;
                
                if (bsonValue.IsArray || bsonValue.IsDocument)
                {
                    loadAction = GetNodes;
                }

                var fieldNode = new DocumentFieldNode(key, bsonValue, loadAction);

                nodes.Add(fieldNode);
            }

            return nodes;
        }

        public IEnumerator<DocumentFieldNode> GetEnumerator()
        {
            return Nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable) Nodes).GetEnumerator();
        }
    }

    public class DocumentFieldNode : INotifyPropertyChanged
    {
        private bool _isExpanded;
        private bool _loaded;

        private Func<BsonDocument, ObservableCollection<DocumentFieldNode>> _loadNodes;

        private DocumentFieldNode()
        {
        }
        
        public DocumentFieldNode(string key, BsonValue value, Func<BsonDocument, ObservableCollection<DocumentFieldNode>> loadNodes)
        {
            Key = key;
            Value = value;

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
            
            _loadNodes = loadNodes;

            if (_loadNodes != null)
            {
                // Add Dummy load node
                Nodes = new ObservableCollection<DocumentFieldNode>
                {
                    new DocumentFieldNode()
                };
            }
        }

        public int? NodesCount { get; set; }

        public string NodesCountText { get; set; }

        public string Key { get; set; }
        
        public BsonValue Value { get; set; }

        public bool IsSelected { get; set; }
        
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

        public event PropertyChangedEventHandler PropertyChanged;
        
        private void OnNodeExpanded()
        {
            if(IsExpanded == false)
                return;
            
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
                    nodes.Add(new DocumentFieldNode(index.ToString(), arrayDoc, _loadNodes));
                    index++;
                }

                Nodes = nodes;
            }
        }

        
    }

}
