﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using LiteDbExplorer.Core;
using LiteDB;

namespace LiteDbExplorer
{
    public class DatabaseReference : ReferenceNode<DatabaseReference>, IDisposable
    {
        private string _name;
        private string _location;
        private ObservableCollection<CollectionReference> _collections;
        private bool _isDisposing;
        private bool _beforeDisposeHandled;

        public DatabaseReference(string path, string password)
        {
            Location = path;
            Name = Path.GetFileName(path);

            LiteDatabase = string.IsNullOrEmpty(password)
                ? new LiteDatabase(path)
                : new LiteDatabase($"Filename={path};Password={password}");

            UpdateCollections();

            OnReferenceChanged(ReferenceNodeChangeAction.Add, this);
        }
        
        public LiteDatabase LiteDatabase { get; }

        public string Name
        {
            get => _name;
            set
            {
                if (value == _name) return;
                OnPropertyChanging();
                _name = value;
                OnPropertyChanged();
            }
        }

        public string Location
        {
            get => _location;
            set
            {
                if (value == _location) return;
                OnPropertyChanging();
                _location = value;
                OnPropertyChanged();
            }
        }
        
        public ObservableCollection<CollectionReference> Collections
        {
            get => _collections;
            set
            {
                OnPropertyChanging();

                if (_collections != null)
                {
                    _collections.CollectionChanged -= OnCollectionChanged;
                }
                
                _collections = value;

                if (_collections != null)
                {
                    _collections.CollectionChanged += OnCollectionChanged;
                }
                OnPropertyChanged();
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems != null)
                    {
                        BroadcastChanges(ReferenceNodeChangeAction.Add, e.NewItems.Cast<CollectionReference>());
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    if (e.OldItems != null)
                    {
                        BroadcastChanges(ReferenceNodeChangeAction.Remove, e.OldItems.Cast<CollectionReference>());
                    }
                    break;
            }
        }

        private void BroadcastChanges(ReferenceNodeChangeAction action, DatabaseReference reference)
        {
            BroadcastChanges(action, Collections);

            OnReferenceChanged(action, reference);
        }

        private void BroadcastChanges(ReferenceNodeChangeAction action, IEnumerable<CollectionReference> items)
        {
            foreach (var referenceCollection in items)
            {
                foreach (var documentReference in referenceCollection.Items)
                {
                    documentReference.OnReferenceChanged(action, documentReference);
                }
                
                referenceCollection.OnReferenceChanged(action, referenceCollection);
            }
        }

        public void BeforeDispose()
        {
            if (_isDisposing)
            {
                return;
            }

            _beforeDisposeHandled = true;

            BroadcastChanges(ReferenceNodeChangeAction.Remove, this);
        }

        public void Dispose()
        {
            if (_isDisposing)
            {
                return;
            }

            _isDisposing = true;

            if (!_beforeDisposeHandled)
            {
                BroadcastChanges(ReferenceNodeChangeAction.Remove, this);
            }
            
            LiteDatabase.Dispose();
        }
        
        private void UpdateCollections()
        {
            var collectionReferences = new ObservableCollection<CollectionReference>(
                LiteDatabase.GetCollectionNames()
                    .Where(a => a != @"_chunks")
                    .OrderBy(a => a)
                    .Select(a => a == @"_files" ? new FileCollectionReference(a, this) : new CollectionReference(a, this))
            );

            Collections = collectionReferences;
        }
        

        public DocumentReference AddFile(string id, string path)
        {
            LiteDatabase.FileStorage.Upload(id, path);
            UpdateCollections();
            var collection = Collections.First(a => a is FileCollectionReference);
            return collection.Items.First(a => a.LiteDocument["_id"] == id);
        }

        public CollectionReference AddCollection(string name)
        {
            if (LiteDatabase.GetCollectionNames().Contains(name))
            {
                throw new Exception($"Cannot add collection \"{name}\", collection with that name already exists.");
            }

            var coll = LiteDatabase.GetCollection(name);
            var newDoc = new BsonDocument
            {
                ["_id"] = ObjectId.NewObjectId()
            };

            coll.Insert(newDoc);
            coll.Delete(newDoc["_id"]);
            UpdateCollections();

            return Collections.FirstOrDefault(p => p.Name.Equals(name));
        }

        public void RenameCollection(string oldName, string newName)
        {
            LiteDatabase.RenameCollection(oldName, newName);
            UpdateCollections();
        }

        public void DropCollection(string name)
        {
            LiteDatabase.DropCollection(name);
            UpdateCollections();
        }

        public static bool IsDbPasswordProtected(string path)
        {
            using (var db = new LiteDatabase(path))
            {
                try
                {
                    db.GetCollectionNames();
                    return false;
                }
                catch (LiteException e)
                {
                    if (e.ErrorCode == LiteException.DATABASE_WRONG_PASSWORD || e.Message.Contains("password"))
                    {
                        return true;
                    }

                    throw;
                }
            }
        }

        public void Refresh()
        {
            UpdateCollections();
        }
    }
}