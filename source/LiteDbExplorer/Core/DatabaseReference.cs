using LiteDB;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace LiteDbExplorer
{
    public class DatabaseReference : INotifyPropertyChanged, IDisposable
    {
        public LiteDatabase LiteDatabase
        {
            get;
        }

        public string Name { get; set; }

        public string Location { get; set; }

        public ObservableCollection<CollectionReference> Collections { get; set; }
        
        public event PropertyChangedEventHandler PropertyChanged;

        public DatabaseReference(string path, string password)
        {            
            Location = path;
            Name = Path.GetFileName(path);

            LiteDatabase = string.IsNullOrEmpty(password) ? 
                new LiteDatabase(path) : 
                new LiteDatabase($"Filename={path};Password={password}");

            UpdateCollections();
        }

        public void Dispose()
        {
            LiteDatabase.Dispose();
        }

        private void UpdateCollections()
        {
            Collections = new ObservableCollection<CollectionReference>(LiteDatabase.GetCollectionNames()
                .Where(a => a != @"_chunks").OrderBy(a => a).Select(a =>
                {
                    if (a == @"_files")
                    {
                        return new FileCollectionReference(a, this);
                    }

                    return new CollectionReference(a, this);
                }));

            OnPropertyChanged(nameof(Collections));
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
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
                    if (e.Message.Contains("password"))
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
