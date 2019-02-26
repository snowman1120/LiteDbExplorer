using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Modules.Database;
using LiteDB;
using LogManager = NLog.LogManager;

namespace LiteDbExplorer.Modules
{
    public class DefaultCommandsHandler : ApplicationCommandHandler
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IDatabaseInteractions _databaseInteractions;
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowManager _windowManager;

        [ImportingConstructor]
        public DefaultCommandsHandler(
            IDatabaseInteractions databaseInteractions, 
            IEventAggregator eventAggregator,
            IWindowManager windowManager)
        {
            _databaseInteractions = databaseInteractions;
            _eventAggregator = eventAggregator;
            _windowManager = windowManager;

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

            Add(ApplicationCommands.Paste, (sender, args) =>
            {
                try
                {
                    var textData = Clipboard.GetText();
                    _databaseInteractions.ImportDataFromText(Store.Current.SelectedCollection, textData);
                }
                catch (Exception exc)
                {
                    Logger.Warn(exc, "Cannot process clipboard data.");
                    MessageBox.Show("Failed to paste document from clipboard: " + exc.Message, "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }, (sender, e) =>
            {
                e.CanExecute = Store.Current.SelectedCollection != null && Store.Current.SelectedCollection.Name != "_files" && Clipboard.ContainsText();
            });


            Add(Commands.EditDbProperties, (sender, args) =>
            {
                _databaseInteractions.ShowDatabaseProperties(Store.Current.SelectedDatabase.LiteDatabase);

            }, (sender, e) =>
            {
                e.CanExecute = Store.Current.HasSelectedDatabase;
            });


            Add(Commands.Exit, (sender, args) =>
            {
                Store.Current.CloseDatabases();

                if (Application.Current.MainWindow != null)
                {
                    Application.Current.MainWindow.Close();
                }
            });

            Add(Commands.Add, (sender, args) =>
            {
                
                var result = _databaseInteractions.CreateItem(Store.Current.SelectedCollection);
                if (result.IsSuccess)
                {
                    var reference = result.Value;
                
                    if (reference.Type == DocumentType.File)
                    {
                        Store.Current.SelectCollection(Store.Current.SelectedCollection.Database.Collections.First(a => a.Name == "_files"));
                        Store.Current.SelectDocument(reference.DocumentReference);
                    }
                    else
                    {
                        Store.Current.SelectDocument(reference.DocumentReference);
                        // UpdateGridColumns(reference.DocumentReference.LiteDocument);
                    }

                    // CollectionListView.ScrollIntoSelectedItem();

                    // UpdateDocumentPreview();
                }

            }, (sender, e) =>
            {
                e.CanExecute = Store.Current.HasSelectedDatabase;
            });

            Add(Commands.Exit, (sender, args) =>
            {
                
            }, (sender, e) =>
            {
                e.CanExecute = true;
            });
        }
    }
}