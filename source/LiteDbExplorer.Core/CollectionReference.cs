using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using LiteDbExplorer.Core;
using LiteDB;

namespace LiteDbExplorer
{
    public class CollectionReference : ReferenceNode<CollectionReference>, IDisposable
    {
        private ObservableCollection<DocumentReference> _items;
        private string _name;
        private DatabaseReference _database;

        public CollectionReference(string name, DatabaseReference database)
        {
            Name = name;
            Database = database;
        }

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                OnPropertyChanging();
                OnPropertyChanging(nameof(LiteCollection));
                _name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(LiteCollection));
            }
        }

        public DatabaseReference Database
        {
            get => _database;
            set
            {
                if (Equals(value, _database)) return;
                _database = value;
                OnPropertyChanging();
                OnPropertyChanging(nameof(LiteCollection));
                OnPropertyChanged();
                OnPropertyChanged(nameof(LiteCollection));
            }
        }

        public ObservableCollection<DocumentReference> Items
        {
            get
            {
                if (_items == null)
                {
                    _items = new ObservableCollection<DocumentReference>();
                    foreach (var item in GetAllItem(LiteCollection))
                    {
                        _items.Add(item);
                    }
                    _items.CollectionChanged += OnDocumentsCollectionChanged;
                }

                return _items;
            }
            set
            {
                OnPropertyChanging(nameof(Items));
                if (_items != null)
                {
                    _items.CollectionChanged -= OnDocumentsCollectionChanged;
                }

                _items = value;
                if (_items != null)
                {
                    _items.CollectionChanged += OnDocumentsCollectionChanged;
                }

                OnPropertyChanged(nameof(Items));
            }
        }

        public event EventHandler<CollectionReferenceChangedEventArgs<DocumentReference>> DocumentsCollectionChanged;

        private void OnDocumentsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        BroadcastChanges(ReferenceNodeChangeAction.Add, e.NewItems.Cast<DocumentReference>());
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    if (e.OldItems != null)
                    {
                        BroadcastChanges(ReferenceNodeChangeAction.Remove, e.OldItems.Cast<DocumentReference>());
                    }
                    break;
            }
        }

        private void BroadcastChanges(ReferenceNodeChangeAction action, IEnumerable<DocumentReference> items)
        {
            foreach (var documentReference in items)
            {
                documentReference.OnReferenceChanged(action, documentReference);
            }

            OnDocumentsCollectionChanged(action, items);
        }

        public LiteCollection<BsonDocument> LiteCollection => Database.LiteDatabase.GetCollection(Name);

        public bool IsFilesOrChunks => IsFilesOrChunksCollection(this);

        public virtual void UpdateItem(DocumentReference document)
        {
            LiteCollection.Update(document.LiteDocument);

            document.OnReferenceChanged(ReferenceNodeChangeAction.Update, document);

            OnDocumentsCollectionChanged(ReferenceNodeChangeAction.Update, new []{ document });
        }

        public virtual void RemoveItem(DocumentReference document)
        {
            LiteCollection.Delete(document.LiteDocument["_id"]);
            Items.Remove(document);
        }

        public virtual void RemoveItems(IEnumerable<DocumentReference> documents)
        {
            foreach (var doc in documents)
            {
                RemoveItem(doc);
            }
        }

        public virtual DocumentReference AddItem(BsonDocument document)
        {
            LiteCollection.Insert(document);
            var newDoc = new DocumentReference(document, this);
            Items.Add(newDoc);
            return newDoc;
        }

        public virtual void Refresh()
        {
            OnPropertyChanging(nameof(Items));

            if (_items == null)
            {
                _items = new ObservableCollection<DocumentReference>();
            }
            else
            {
                _items.Clear();
            }

            foreach (var item in GetAllItem(LiteCollection))
            {
                _items.Add(item);
            }

            OnPropertyChanged(nameof(Items));
        }

        public IReadOnlyList<string> GetDistinctKeys(FieldSortOrder sortOrder = FieldSortOrder.Original)
        {
            var keys = Items
                .SelectMany(p => p.LiteDocument.Keys)
                .Distinct(StringComparer.InvariantCulture);

            if (sortOrder == FieldSortOrder.Alphabetical)
            {
                keys = keys.OrderBy(_ => _);
            }

            return keys.ToList();
        }

        public static bool IsFilesOrChunksCollection(CollectionReference reference)
        {
            if (reference == null)
            {
                return false;
            }

            return reference.Name == @"_files" || reference.Name == @"_chunks";
        }

        protected virtual IEnumerable<DocumentReference> GetAllItem(LiteCollection<BsonDocument> liteCollection)
        {
            return LiteCollection.FindAll().Select(bsonDocument => new DocumentReference(bsonDocument, this));
        }

        protected virtual void OnDocumentsCollectionChanged(ReferenceNodeChangeAction action, IEnumerable<DocumentReference> items)
        {
            DocumentsCollectionChanged?.Invoke(this, new CollectionReferenceChangedEventArgs<DocumentReference>(action, items));
        }

        public void Dispose()
        {
            Items = null;
            Database = null;
        }
    }
}