using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace LiteDbExplorer
{
    public class Store : INotifyPropertyChanged
    {
        private static readonly Lazy<Store> _current = new Lazy<Store>(() => new Store());

        private CollectionReference _selectedCollection;
        private DatabaseReference _selectedDatabase;
        private DocumentReference _selectedDocument;
        private IList<DocumentReference> _selectedDocuments;

        public event EventHandler<EventArgs<DatabaseReference>> SelectedDatabaseChange;
        public event EventHandler<EventArgs<CollectionReference>> SelectedCollectionChange;
        public event EventHandler<EventArgs<DocumentReference>> SelectedDocumentChange;

        private Store()
        {
            Databases = new ObservableCollection<DatabaseReference>();
        }

        public static Store Current => _current.Value;

        public ObservableCollection<DatabaseReference> Databases { get; private set; }

        public DatabaseReference SelectedDatabase
        {
            get => _selectedDatabase;
            private set
            {
                _selectedDatabase = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedDatabase));
            }
        }

        public CollectionReference SelectedCollection
        {
            get => _selectedCollection;
            private set
            {
                _selectedCollection = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedCollection));
            }
        }

        public DocumentReference SelectedDocument
        {
            get => _selectedDocument;
            set
            {
                if (Equals(value, _selectedDocument)) return;
                _selectedDocument = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelectedDocument));
            }
        }

        public IList<DocumentReference> SelectedDocuments
        {
            get => _selectedDocuments;
            set
            {
                if (Equals(value, _selectedDocuments)) return;
                _selectedDocuments = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedDocumentsCount));
            }
        }

        public bool HasSelectedDatabase => SelectedDatabase != null;

        public bool HasSelectedCollection => SelectedCollection != null;

        public bool HasSelectedDocument => SelectedDocument != null;

        public int SelectedDocumentsCount => SelectedDocuments?.Count ?? 0;

        public void AddDatabase(DatabaseReference databaseReference)
        {
            Databases.Add(databaseReference);
        }

        public void CloseDatabase(string instanceId)
        {
            var databaseReference = GetDatabaseReference(instanceId);
            CloseDatabase(databaseReference);
        }
        
        public void CloseDatabase(DatabaseReference databaseReference)
        {
            if(databaseReference == null)
            {
                return;
            }

            if (SelectedCollection?.Database == databaseReference)
            {
                SelectedCollection = null;
            }

            if (SelectedDatabase == databaseReference)
            {
                SelectedDatabase = null;
            }

            databaseReference?.BeforeDispose();
            
            Databases.Remove(databaseReference);

            databaseReference?.Dispose();
        }

        public void CloseSelectedDatabase()
        {
            CloseDatabase(SelectedDatabase);
        }

        public void CloseDatabases()
        {
            SelectedCollection = null;
            SelectedDatabase = null;

            foreach (var db in Databases)
            {
                db.Dispose();
            }

            Databases = new ObservableCollection<DatabaseReference>();
        }

        public DatabaseReference GetDatabaseReference(string instanceId)
        {
            var dbReference = Databases.FirstOrDefault(p => p.InstanceId.Equals(instanceId));
            if(dbReference != null)
            {
                return dbReference;
            }

            return Databases.FirstOrDefault(
                p => p.Collections.Any(c => c.InstanceId.Equals(instanceId))
            );
        }

        public bool IsDatabaseOpen(string path)
        {
            return Databases.FirstOrDefault(a => a.Location == path) != null;
        }

        public void ResetSelectedCollection()
        {
            SelectedCollection = null;
        }

        public void SelectCollection(CollectionReference collectionReference)
        {
            SelectedCollection = collectionReference;
        }

        public void SelectDocument(DocumentReference documentReference)
        {
            SelectedDocument = documentReference;
        }

        public void SelectDatabase(DatabaseReference databaseReference)
        {
            SelectedDatabase = databaseReference;
        }

        public void SelectNode(IReferenceNode value)
        {
            if (value == null)
            {
                ResetSelectedCollection();
                return;
            }

            if (value is CollectionReference collection)
            {
                SelectCollection(collection);
                SelectDatabase(collection.Database);
            }
            else if (value is DatabaseReference reference)
            {
                SelectDatabase(reference);
            }
            else
            {
                ResetSelectedCollection();
            }
        }

        public void SelectNode(DbNavigationNodeType nodeType, string instanceId)
        {
            switch (nodeType)
            {
                case DbNavigationNodeType.Database:
                {
                    SelectedDatabase = Databases.FirstOrDefault(p => p.InstanceId.Equals(instanceId));
                    break;
                }
                case DbNavigationNodeType.Collection:
                case DbNavigationNodeType.FileCollection:
                {
                    var selectedCollection = Databases
                        .SelectMany(p => p.Collections)
                        .FirstOrDefault(p => p.InstanceId.Equals(instanceId));

                    SelectedDatabase = selectedCollection?.Database;
                    SelectedCollection = selectedCollection;

                    break;
                }
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

            switch (propertyName)
            {
                case nameof(SelectedDatabase):
                    SelectedDatabaseChange?.Invoke(this, new EventArgs<DatabaseReference>(_selectedDatabase));
                    break;
                case nameof(SelectedCollection):
                    SelectedCollectionChange?.Invoke(this, new EventArgs<CollectionReference>(_selectedCollection));
                    break;
                case nameof(SelectedDocument):
                    SelectedDocumentChange?.Invoke(this, new EventArgs<DocumentReference>(_selectedDocument));
                    break;
            }
        }
    }
}