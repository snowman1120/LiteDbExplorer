using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LiteDbExplorer.Framework;
using LiteDB;

namespace LiteDbExplorer.Modules
{
    public class DatabaseCommandsHandler : ApplicationCommandHandler
    {
        private readonly IDatabaseInteractions _databaseInteractions;

        [ImportingConstructor]
        public DatabaseCommandsHandler(IDatabaseInteractions databaseInteractions)
        {
            _databaseInteractions = databaseInteractions;
   
            Add(ApplicationCommands.Open, (sender, args) =>
            {
                _databaseInteractions.OpenDatabase();
            });

            Add(ApplicationCommands.New, (sender, args) =>
            {
                _databaseInteractions.CreateAndOpenDatabase();
            });

            Add(ApplicationCommands.Close, (sender, args) =>
            {
                Store.Current.CloseSelectedDatabase();
            }, (sender, e) =>
            {
                e.CanExecute = Store.Current.HasSelectedDatabase;
            });

            Add(ApplicationCommands.Copy, (sender, args) =>
            {
                _databaseInteractions.CopySelectedDocuments();

            }, (sender, e) =>
            {
                e.CanExecute = Store.Current.SelectedDocumentsCount > 0 && Store.Current.SelectedCollection != null && Store.Current.SelectedCollection.Name != "_files";
            });
        }
    }
}