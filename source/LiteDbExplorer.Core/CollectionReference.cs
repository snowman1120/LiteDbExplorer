using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using LiteDB;

namespace LiteDbExplorer
{
    public class CollectionReference : INotifyPropertyChanging, INotifyPropertyChanged, IReferenceNode
    {
        private ObservableCollection<DocumentReference> _items;
        private string _name;
        private DatabaseReference _database;

        public CollectionReference(string name, DatabaseReference database)
        {
            InstanceId = Guid.NewGuid().ToString();
            Name = name;
            Database = database;
        }

        public string InstanceId { get; }

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
                    foreach (var item in LiteCollection.FindAll().Select(a => new DocumentReference(a, this)))
                    {
                        _items.Add(item);
                    }
                }

                return _items;
            }
            set
            {
                OnPropertyChanging(nameof(Items));
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public LiteCollection<BsonDocument> LiteCollection => Database.LiteDatabase.GetCollection(Name);

        public bool IsFilesOrChunks => IsFilesOrChunksCollection(this);

        public bool InstanceEquals(CollectionReference collectionReference)
        {
            return InstanceId.Equals(collectionReference?.InstanceId);
        }
        
        public virtual void UpdateItem(DocumentReference document)
        {
            LiteCollection.Update(document.LiteDocument);
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

            foreach (var item in LiteCollection.FindAll().Select(a => new DocumentReference(a, this)))
            {
                _items.Add(item);
            }

            OnPropertyChanged(nameof(Items));
        }

        public void InvalidateProperties()
        {
            if (Items != null)
            {
                foreach (var documentReference in Items) documentReference.InvalidateProperties();
            }

            OnPropertyChanged(string.Empty);
        }

        public static bool IsFilesOrChunksCollection(CollectionReference reference)
        {
            if (reference == null)
            {
                return false;
            }

            return reference.Name == @"_files" || reference.Name == @"_chunks";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangingEventHandler PropertyChanging;

        protected virtual void OnPropertyChanging([CallerMemberName] string name = null)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(name));
        }
    }
}