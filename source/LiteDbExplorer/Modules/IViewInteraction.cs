using System;
using System.Collections.Generic;
using LiteDB;

namespace LiteDbExplorer.Modules
{
    public interface IViewInteraction
    {
        bool OpenDatabaseProperties(DatabaseReference database);
        bool OpenEditDocument(DocumentReference document);
        bool RevealInExplorer(string filePath);
        void ActivateDocument(DocumentReference document);
        void ActivateCollection(CollectionReference collection, IEnumerable<DocumentReference> selectedDocuments = null);
        void ShowError(Exception exception, string message, string title = "");
        bool ShowConfirm(string message, string title = "Are you sure?");
        void ShowError(string message, string title = "");
        void PutClipboardText(string text);
    }
}