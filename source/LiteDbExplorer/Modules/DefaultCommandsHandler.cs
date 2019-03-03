using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Framework.Services;
using LogManager = NLog.LogManager;

namespace LiteDbExplorer.Modules
{
    public class DefaultCommandsHandler : ApplicationCommandHandler
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IDatabaseInteractions _databaseInteractions;

        [ImportingConstructor]
        public DefaultCommandsHandler(
            IDatabaseInteractions databaseInteractions)
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
                _databaseInteractions.CopyDocuments(Store.Current.SelectedDocuments);

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
                _databaseInteractions.OpenDatabaseProperties(Store.Current.SelectedDatabase.LiteDatabase);

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

            Add(Commands.AddFile, (sender, args) =>
            {

                var database = Store.Current.SelectedDatabase;
                var result = _databaseInteractions.AddFileToDatabase(database);
                if (result.IsSuccess)
                {
                    Store.Current.SelectCollection(database.Collections.First(a => a.Name == "_files"));
                    Store.Current.SelectDocument(result.Value.DocumentReference);

                    // CollectionListView.ScrollIntoSelectedItem();
                }
                
            }, (sender, e) =>
            {
                e.CanExecute = Store.Current.HasSelectedDatabase;
            });

            Add(Commands.Edit, (sender, args) =>
            {
                var item = Store.Current.SelectedDocument;

                var document = _databaseInteractions.OpenEditDocument(item);
                if (document.HasValue)
                {
                    // UpdateGridColumns(document.Value.LiteDocument);
                    // UpdateDocumentPreview();
                }
                
                
            }, (sender, e) =>
            {
                e.CanExecute = Store.Current.HasSelectedDocument;
            });

            Add(Commands.Remove, (sender, args) =>
            {
                var currentSelectedDocuments = Store.Current.SelectedDocuments.ToList();
                _databaseInteractions.RemoveDocuments(currentSelectedDocuments);
            }, (sender, e) =>
            {
                e.CanExecute = Store.Current.SelectedDocumentsCount > 0;
            });

            Add(Commands.Export, (sender, args) =>
            {
                _databaseInteractions.ExportDocuments(Store.Current.SelectedDocuments);
            }, (sender, e) =>
            {
                e.CanExecute = Store.Current.SelectedDocumentsCount > 0;
            });

            Add(Commands.RefreshCollection, (sender, args) =>
            {
                Store.Current.SelectedCollection?.Refresh();
            }, (sender, e) =>
            {
                e.CanExecute = Store.Current.HasSelectedCollection;
            });

            Add(Commands.AddCollection, (sender, args) =>
            {
                _databaseInteractions.AddCollection(Store.Current.SelectedDatabase);
            }, (sender, e) =>
            {
                e.CanExecute = Store.Current.HasSelectedDatabase;
            });

            Add(Commands.RenameCollection, (sender, args) =>
            {
                _databaseInteractions.RenameCollection(Store.Current.SelectedCollection);
            }, (sender, e) =>
            {
                e.CanExecute = Store.Current.SelectedCollection != null && Store.Current.SelectedCollection.Name != "_files" && Store.Current.SelectedCollection.Name != "_chunks";
            });

            Add(Commands.DropCollection, (sender, args) =>
            {
                var result = _databaseInteractions.DropCollection(Store.Current.SelectedCollection);
                if (result.IsSuccess && result.Value)
                {
                    Store.Current.ResetSelectedCollection();
                }
            }, (sender, e) =>
            {
                e.CanExecute = Store.Current.SelectedCollection != null && Store.Current.SelectedCollection.Name != "_files" && Store.Current.SelectedCollection.Name != "_chunks";
            });


            Add(Commands.ExportCollection, (sender, args) =>
            {
                _databaseInteractions.ExportCollection(Store.Current.SelectedCollection);   
            }, (sender, e) =>
            {
                e.CanExecute = Store.Current.SelectedCollection != null && Store.Current.SelectedCollection.Name != "_files" && Store.Current.SelectedCollection.Name != "_chunks";
            });


            Add(Commands.RefreshDatabase, (sender, args) =>
            {
                Store.Current.ResetSelectedCollection();
                Store.Current.SelectedDatabase.Refresh();   
            }, (sender, e) =>
            {
                e.CanExecute = Store.Current.HasSelectedDatabase;
            });
        }
    }
}