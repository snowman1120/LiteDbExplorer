using System;
using System.Collections.Generic;
using LiteDB;

namespace LiteDbExplorer
{
    public enum DocumentTypeFilter
    {
        All = -1,
        BsonDocument = 0,
        File = 1
    }

    public enum DocumentType
    {
        BsonDocument = 0,
        File = 1
    }

    public class DocumentReference : ReferenceNode<DocumentReference>, IDisposable
    {
        private BsonDocument _liteDocument;
        private CollectionReference _collection;

        public DocumentReference()
        {
        }

        public DocumentReference(BsonDocument document, CollectionReference collection) : this()
        {
            LiteDocument = document;
            Collection = collection;
        }

        public BsonDocument LiteDocument
        {
            get => _liteDocument;
            set
            {
                OnPropertyChanging();
                _liteDocument = value;
                OnPropertyChanged();
            }
        }

        public CollectionReference Collection
        {
            get => _collection;
            set
            {
                OnPropertyChanging();
                _collection = value;
                OnPropertyChanged();
            }
        }
        
        public bool ContainsReference(CollectionReference collectionReference)
        {
            if (Collection == null)
            {
                return false;
            }

            return Collection.InstanceId.Equals(collectionReference?.InstanceId);
        }

        public void RemoveSelf()
        {
            Collection?.RemoveItem(this);
        }

        public void Dispose()
        {
            LiteDocument = null;
            Collection = null;
        }
    }
}