using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using LiteDB;

namespace LiteDbExplorer
{
    public class DatabaseReference : INotifyPropertyChanging, INotifyPropertyChanged, IDisposable, IReferenceNode
    {
        private string _name;
        private string _location;
        private ObservableCollection<CollectionReference> _collections;

        public DatabaseReference(string path, string password)
        {
            InstanceId = Guid.NewGuid().ToString();
            Location = path;
            Name = Path.GetFileName(path);

            LiteDatabase = string.IsNullOrEmpty(password)
                ? new LiteDatabase(path)
                : new LiteDatabase($"Filename={path};Password={password}");

            UpdateCollections();
        }

        public string InstanceId { get; }

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
                if (Equals(value, _collections)) return;
                OnPropertyChanging();
                _collections = value;
                OnPropertyChanged();
            }
        }

        public void Dispose()
        {
            LiteDatabase.Dispose();
        }
        
        private void UpdateCollections()
        {
            Collections = new ObservableCollection<CollectionReference>(
                LiteDatabase.GetCollectionNames()
                .Where(a => a != @"_chunks")
                .OrderBy(a => a)
                .Select(a => a == @"_files" ? new FileCollectionReference(a, this) : new CollectionReference(a, this))
               );
        }
        

        public DocumentReference AddFile(string id, string path)
        {
            LiteDatabase.FileStorage.Upload(id, path);
            UpdateCollections();
            var collection = Collections.First(a => a is FileCollectionReference);
            return collection.Items.First(a => a.LiteDocument["_id"] == id);
        }

        public void AddCollection(string name)
        {
            if (LiteDatabase.GetCollectionNames().Contains(name))
                throw new Exception($"Cannot add collection \"{name}\", collection with that name already exists.");

            var coll = LiteDatabase.GetCollection(name);
            var newDoc = new BsonDocument
            {
                ["_id"] = ObjectId.NewObjectId()
            };

            coll.Insert(newDoc);
            coll.Delete(newDoc["_id"]);
            UpdateCollections();
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
                    if (e.Message.Contains("password")) return true;

                    throw;
                }
            }
        }

        public void Refresh()
        {
            UpdateCollections();
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