using System.Collections.Generic;
using LiteDB;

namespace LiteDbExplorer.Modules
{
    public interface IViewInteractionResolver
    {
        bool OpenDatabaseProperties(DatabaseReference database);
        bool OpenEditDocument(DocumentReference document);
        bool RevealInExplorer(string filePath);
        void ActivateDocument(DocumentReference document);
        void ActivateCollection(CollectionReference collection, IEnumerable<DocumentReference> selectedDocuments = null);
    }
}