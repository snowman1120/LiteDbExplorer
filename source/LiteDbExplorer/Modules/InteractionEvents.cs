using System.Collections.Generic;
using LiteDB;

namespace LiteDbExplorer.Modules
{
    public class InteractionEvents
    {
        public class DocumentUpdated
        {
            public DocumentUpdated(DocumentReference documentReference)
            {
                DocumentReference = documentReference;
            }

            public DocumentReference DocumentReference { get; }
        }

        public abstract class CollectionEvent
        {
            protected CollectionEvent(CollectionReference collectionReference)
            {
                CollectionReference = collectionReference;
            }

            public CollectionReference CollectionReference { get; }
        }

        public class CollectionUpdated : CollectionEvent
        {
            public CollectionUpdated(CollectionReference collectionReference) : base(collectionReference)
            {
            }
        }

        public class CollectionRemoved : CollectionEvent
        {
            public CollectionRemoved(CollectionReference collectionReference) : base(collectionReference)
            {
            }
        }

        public class CollectionDocumentsCreated : CollectionEvent
        {
            public CollectionDocumentsCreated(CollectionReference collectionReference, IEnumerable<DocumentReference> newDocuments) : base(collectionReference)
            {
                NewDocuments = newDocuments;
            }

            public IEnumerable<DocumentReference> NewDocuments { get; }
        }

        public class DatabaseClosed
        {
            public DatabaseClosed(DatabaseReference databaseReference)
            {
                DatabaseReference = databaseReference;
            }

            public DatabaseReference DatabaseReference { get; }
        }
    }
}