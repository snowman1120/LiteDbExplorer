using System.Collections.Generic;
using System.Linq;
using System.Windows;
using LiteDbExplorer.Controls;
using LiteDbExplorer.Modules;
using LiteDbExplorer.Windows;
using LiteDB;

namespace LiteDbExplorer
{
    public class MainWindowViewInteractionResolver : IViewInteractionResolver
    {
        public Window Owner { get; set; }

        public bool OpenDatabaseProperties(LiteDatabase database)
        {
            var window = new DatabasePropertiesWindow(Store.Current.SelectedDatabase.LiteDatabase)
            {
                Owner = Owner
            };

            return window.ShowDialog() == true;
        }

        public bool OpenEditDocument(DocumentReference document)
        {
            var windowController = new WindowController {Title = "Document Editor"};
            var control = new DocumentEntryControl(document, windowController);
            var window = new DialogWindow(control, windowController)
            {
                Owner = Owner,
                Height = 600
            };

            return window.ShowDialog() == true;
        }

        public bool RevealInExplorer(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return false;
            }

            //Clean up file path so it can be navigated OK
            filePath = System.IO.Path.GetFullPath(filePath);
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");
            return true;
        }

        public void ActivateDocument(DocumentReference document)
        {
            Store.Current.SelectCollection(document.Collection);
            Store.Current.SelectDocument(document);
        }

        public void ActivateCollection(CollectionReference collection, IEnumerable<DocumentReference> selectedDocuments = null)
        {
            Store.Current.SelectCollection(collection);
            Store.Current.SelectDocument(selectedDocuments?.FirstOrDefault());
            Store.Current.SelectedDocuments = selectedDocuments.ToList();
        }
    }
}