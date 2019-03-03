using System.Windows;
using LiteDbExplorer.Controls;
using LiteDbExplorer.Modules;
using LiteDbExplorer.Windows;
using LiteDB;

namespace LiteDbExplorer
{
    public class MainWindowInteractionResolver : IInteractionResolver
    {
        public Window Owner { get; set; }

        public bool ShowDatabaseProperties(LiteDatabase database)
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
    }
}