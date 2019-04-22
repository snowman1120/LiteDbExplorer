using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LiteDbExplorer.Core;

namespace LiteDbExplorer.Modules
{
    public abstract class LiteDbReferenceCollectionChangeEvent<T> where T : IReferenceNode
    {
        protected LiteDbReferenceCollectionChangeEvent()
        {
            Action = ReferenceNodeChangeAction.None;
            Items = new T[0];
        }

        protected LiteDbReferenceCollectionChangeEvent(ReferenceNodeChangeAction action, IReadOnlyCollection<T> items)
        {
            Action = action;
            Items = items;
        }

        protected LiteDbReferenceCollectionChangeEvent(ReferenceNodeChangeAction action, T item) 
            : this(action, new []{ item })
        {

        }
        
        public virtual ReferenceNodeChangeAction Action { get; }

        public virtual IReadOnlyCollection<T> Items { get; }

        public virtual bool ContainsReference(T item)
        {
            return Items != null && Items.Any(p => p.InstanceId.Equals(item.InstanceId));
        }
    }

    public class InteractionEvents
    {
        public class DatabaseChange : LiteDbReferenceCollectionChangeEvent<DatabaseReference>
        {
            public static DatabaseChange Nome = new DatabaseChange();

            private DatabaseChange()
            {
            }

            public DatabaseChange(ReferenceNodeChangeAction action, IReadOnlyCollection<DatabaseReference> items) : base(action, items)
            {
            }

            public DatabaseChange(ReferenceNodeChangeAction action, DatabaseReference item) : base(action, item)
            {
            }
        }

        public class CollectionChange : LiteDbReferenceCollectionChangeEvent<CollectionReference>
        {
            public static CollectionChange Nome = new CollectionChange();

            private CollectionChange()
            {
            }

            public CollectionChange(ReferenceNodeChangeAction action, IReadOnlyCollection<CollectionReference> items) : base(action, items)
            {
            }

            public CollectionChange(ReferenceNodeChangeAction action, CollectionReference item) : base(action, item)
            {
            }
        }

        public class CollectionDocumentChange : LiteDbReferenceCollectionChangeEvent<DocumentReference>
        {
            public static CollectionDocumentChange Nome = new CollectionDocumentChange();

            private CollectionDocumentChange()
            {
            }

            public CollectionDocumentChange(ReferenceNodeChangeAction action, IReadOnlyCollection<DocumentReference> items, CollectionReference collectionReference) : base(action, items)
            {
                CollectionReference = collectionReference;
            }

            public CollectionDocumentChange(ReferenceNodeChangeAction action, DocumentReference item, CollectionReference collectionReference) : base(action, item)
            {
                CollectionReference = collectionReference ?? item?.Collection;
            }

            public CollectionReference CollectionReference { get; }
        }

        public class DocumentChange : LiteDbReferenceCollectionChangeEvent<DocumentReference>
        {
            public static DocumentChange Nome = new DocumentChange();

            private DocumentChange()
            {
            }

            public DocumentChange(ReferenceNodeChangeAction action, IReadOnlyCollection<DocumentReference> items) : base(action, items)
            {
            }

            public DocumentChange(ReferenceNodeChangeAction action, DocumentReference item) : base(action, item)
            {
            }
        }
    }
}