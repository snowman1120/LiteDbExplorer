using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
using LiteDbExplorer.Core;
using LiteDbExplorer.Windows;
using LiteDB;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using LogManager = NLog.LogManager;

namespace LiteDbExplorer.Modules
{
    public interface IDatabaseInteractions
    {
        Paths PathDefinitions { get; }
        void CreateAndOpenDatabase();
        void OpenDatabase();
        void OpenDatabase(string path, string password = "");
        void OpenDatabases(IEnumerable<string> paths);
        void ExportCollection(CollectionReference collectionReference);
        void ExportDocuments(ICollection<DocumentReference> documents);
        Result<InteractionEvents.CollectionDocumentChange> AddFileToDatabase(DatabaseReference database);
        Result<InteractionEvents.CollectionDocumentChange> ImportDataFromText(CollectionReference collection, string textData);
        Result<InteractionEvents.CollectionDocumentChange> CreateItem(CollectionReference collection);
        Result CopyDocuments(IEnumerable<DocumentReference> documents);
        Maybe<DocumentReference> OpenEditDocument(DocumentReference document);
        Result<CollectionReference> AddCollection(DatabaseReference database);
        Result RenameCollection(CollectionReference collection);
        Result<CollectionReference> DropCollection(CollectionReference collection);
        Result RemoveDocuments(IEnumerable<DocumentReference> documents);
        Result<bool> RevealInExplorer(DatabaseReference database);
        void CloseDatabase(DatabaseReference database);
    }

    [Export(typeof(IDatabaseInteractions))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public class DatabaseInteractions : IDatabaseInteractions
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IViewInteractionResolver _viewInteractionResolver;
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        [ImportingConstructor]
        public DatabaseInteractions(
            IEventAggregator eventAggregator, 
            IViewInteractionResolver viewInteractionResolver)
        {
            _eventAggregator = eventAggregator;
            _viewInteractionResolver = viewInteractionResolver;
            PathDefinitions = new Paths();
        }

        public Paths PathDefinitions { get; }

        public void CreateAndOpenDatabase()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "All files|*.*",
                OverwritePrompt = true
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            using (var stream = new FileStream(dialog.FileName, System.IO.FileMode.Create))
            {
                LiteEngine.CreateDatabase(stream);
            }

            OpenDatabase(dialog.FileName);
        }

        public void OpenDatabase()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "All files|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                OpenDatabase(dialog.FileName);
            }
            catch (Exception exc)
            {
                Logger.Error(exc, "Failed to open database: ");
                ErrorInteraction("Failed to open database: " + exc.Message);
            }
        }

        public void OpenDatabases(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                OpenDatabase(path);
            }
        }

        public void OpenDatabase(string path, string password = "")
        {
            if (Store.Current.IsDatabaseOpen(path))
            {
                return;
            }

            if (!File.Exists(path))
            {
                ErrorInteraction("Cannot open database, file not found.", "File not found");
                return;
            }
            
            try
            {
                if (DatabaseReference.IsDbPasswordProtected(path) && 
                    InputBoxWindow.ShowDialog("Database is password protected, enter password:", "Database password.", password, out password) != true)
                {
                    return;
                }

                Store.Current.AddDatabase(new DatabaseReference(path, password));

                PathDefinitions.InsertRecentFile(path);
            }
            catch (LiteException liteException)
            {
                OpenDatabaseExceptionHandler(liteException, path, password);
            }
            catch (NotSupportedException notSupportedException)
            {
                ErrorInteraction("Failed to open database [NotSupportedException]:" + Environment.NewLine + notSupportedException.Message);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to open database: ");
                ErrorInteraction("Failed to open database [Exception]:" + Environment.NewLine + e.Message);
            }
        }

        protected virtual void OpenDatabaseExceptionHandler(LiteException liteException, string path, string password = "")
        {
            if (liteException.ErrorCode == LiteException.DATABASE_WRONG_PASSWORD)
            {
                if (!string.IsNullOrEmpty(password))
                {
                    ErrorInteraction("Failed to open database [LiteException]:" + Environment.NewLine + liteException.Message);
                }
                    
                OpenDatabase(path, password);
            }
        }
        
        public void CloseDatabase(DatabaseReference database)
        {
            _eventAggregator.PublishOnUIThread(new InteractionEvents.DatabaseChange(ReferenceNodeChangeAction.Remove, database));
            Store.Current.CloseDatabase(database);
        }
        
        public Result<InteractionEvents.CollectionDocumentChange> AddFileToDatabase(DatabaseReference database)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "All files|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
            {
                return Result.Fail<InteractionEvents.CollectionDocumentChange>("FILE_OPEN_CANCELED");
            }

            try
            {
                if (InputBoxWindow.ShowDialog("New file id:", "Enter new file id", Path.GetFileName(dialog.FileName),
                        out string id) == true)
                {
                    var file = database.AddFile(id, dialog.FileName);

                    var documentsCreated = new InteractionEvents.CollectionDocumentChange(ReferenceNodeChangeAction.Add, new [] {file}, file.Collection);

                    _eventAggregator.PublishOnUIThread(documentsCreated);

                    return Result.Ok(documentsCreated);
                }
            }
            catch (Exception exc)
            {
                ErrorInteraction("Failed to upload file:" + Environment.NewLine + exc.Message);
            }


            return Result.Fail<InteractionEvents.CollectionDocumentChange>("FILE_OPEN_FAIL");
        }

        public Result RemoveDocuments(IEnumerable<DocumentReference> documents)
        {
            if (!ConfirmInteraction("Are you sure you want to remove items?"))
            {
                return Result.Fail(Fails.Canceled);
            }

            foreach (var document in documents.ToList())
            {
                document.RemoveSelf();
            }
            
            return Result.Ok();
        }

        public Result<CollectionReference> AddCollection(DatabaseReference database)
        {
            try
            {
                if (InputBoxWindow.ShowDialog("New collection name:", "Enter new collection name", "", out string name) == true)
                {
                    var collectionReference = database.AddCollection(name);

                    return Result.Ok(collectionReference);
                }

                return Result.Ok<CollectionReference>(null);
            }
            catch (Exception exc)
            {
                var message = "Failed to add new collection:" + Environment.NewLine + exc.Message;
                ErrorInteraction(message);
                return Result.Fail<CollectionReference>(message);
            }
        }

        public Result RenameCollection(CollectionReference collection)
        {
            try
            {
                var currentName = collection.Name;
                if (InputBoxWindow.ShowDialog("New name:", "Enter new collection name", currentName, out string name) == true)
                {
                    collection.Database.RenameCollection(currentName, name);
                    return Result.Ok(true);
                }

                return Result.Fail(Fails.Canceled);
            }
            catch (Exception exc)
            {
                var message = "Failed to rename collection:" + Environment.NewLine + exc.Message;
                ErrorInteraction(message);
                return Result.Fail(message);
            }
        }

        public Result<CollectionReference> DropCollection(CollectionReference collection)
        {
            try
            {
                var collectionName = collection.Name;
                if (ConfirmInteraction($"Are you sure you want to drop collection \"{collectionName}\"?"))
                {
                    collection.Database.DropCollection(collectionName);

                    _eventAggregator.PublishOnUIThread(new InteractionEvents.CollectionChange(ReferenceNodeChangeAction.Remove, collection));
                    
                    return Result.Ok(collection);
                }

                return Result.Fail<CollectionReference>(Fails.Canceled);
            }
            catch (Exception exc)
            {
                var message = "Failed to drop collection:" + Environment.NewLine + exc.Message;
                ErrorInteraction(message);
                return Result.Fail<CollectionReference>(message);
            }
        }

        public void ExportCollection(CollectionReference collectionReference)
        {
            if (collectionReference == null)
            {
                return;
            }

            if (collectionReference.IsFilesOrChunks)
            {
                var folderDialog = new CommonOpenFileDialog
                {
                    IsFolderPicker = true,
                    Title = "Select folder to export files to..."
                };

                if (folderDialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    foreach (var file in collectionReference.Items)
                    {
                        var path = Path.Combine(folderDialog.FileName, $"{file.LiteDocument["_id"].AsString}-{file.LiteDocument["filename"].AsString}");
                        var dir = Path.GetDirectoryName(path);
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }

                        (file.Collection as FileCollectionReference)?.SaveFile(file, path);
                    }
                }
            }
            else
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Json File|*.json",
                    FileName = $"{collectionReference.Name}_export.json",
                    OverwritePrompt = true
                };

                if (dialog.ShowDialog() == true)
                {
                    var data = new BsonArray(collectionReference.LiteCollection.FindAll());

                    using (var writer = new StreamWriter(dialog.FileName))
                    {
                        JsonSerializer.Serialize(data, writer, true, false);
                    }

                    // File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(data, true, false));
                }   
            }
        }

        public void ExportDocuments(ICollection<DocumentReference> documents)
        {
            if (documents == null || !documents.Any())
            {
                return;
            }

            var documentReference = documents.First();

            if (documentReference.Collection is FileCollectionReference)
            {
                if (documents.Count == 1)
                {
                    var file = documentReference;

                    var dialog = new SaveFileDialog
                    {
                        Filter = "All files|*.*",
                        FileName = file.LiteDocument["filename"],
                        OverwritePrompt = true
                    };

                    if (dialog.ShowDialog() == true)
                    {
                        (file.Collection as FileCollectionReference)?.SaveFile(file, dialog.FileName);
                    }
                }
                else
                {
                    var dialog = new CommonOpenFileDialog
                    {
                        IsFolderPicker = true,
                        Title = "Select folder to export files to..."
                    };

                    if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                    {
                        foreach (var file in documents)
                        {
                            var path = Path.Combine(dialog.FileName,
                                $"{file.LiteDocument["_id"].AsString}-{file.LiteDocument["filename"].AsString}");
                            var dir = Path.GetDirectoryName(path);
                            if (!Directory.Exists(dir))
                            {
                                Directory.CreateDirectory(dir);
                            }

                            (file.Collection as FileCollectionReference)?.SaveFile(file, path);
                        }
                    }
                }
            }
            else
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Json File|*.json",
                    FileName = "export.json",
                    OverwritePrompt = true
                };

                if (dialog.ShowDialog() == true)
                {
                    if (documents.Count == 1)
                    {

                        using (var writer = new StreamWriter(dialog.FileName))
                        {
                            JsonSerializer.Serialize(documentReference.LiteDocument, writer, true, false);
                        }

                        // File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(documentReference.LiteDocument, true, false));
                    }
                    else
                    {
                        var data = new BsonArray(documents.Select(a => a.LiteDocument));
                        using (var writer = new StreamWriter(dialog.FileName))
                        {
                            JsonSerializer.Serialize(data, writer, true, false);
                        }
                        // File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(data, true, false));
                    }
                }
            }
        }

        public Result CopyDocuments(IEnumerable<DocumentReference> documents)
        {
            var data = new BsonArray(documents.Select(a => a.LiteDocument));
            
            Clipboard.SetData(DataFormats.Text, JsonSerializer.Serialize(data, true, false));

            return Result.Ok();
        }
        
        public Maybe<DocumentReference> OpenEditDocument(DocumentReference document)
        {
            var result = _viewInteractionResolver.OpenEditDocument(document);
            if (result)
            {
                _eventAggregator.PublishOnUIThread(new InteractionEvents.DocumentChange(ReferenceNodeChangeAction.Update, document));
                return document;
            }
            return null;
        }
        
        public Result<InteractionEvents.CollectionDocumentChange> ImportDataFromText(CollectionReference collection, string textData)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textData))
                {
                    return Result.Ok(InteractionEvents.CollectionDocumentChange.Nome);
                }

                var newValue = JsonSerializer.Deserialize(textData);
                var newDocs = new List<DocumentReference>();
                if (newValue.IsArray)
                {
                    foreach (var value in newValue.AsArray)
                    {
                        var doc = value.AsDocument;
                        var documentReference = collection.AddItem(doc);
                        newDocs.Add(documentReference);
                    }
                }
                else
                {
                    var doc = newValue.AsDocument;
                    var documentReference = collection.AddItem(doc);
                    newDocs.Add(documentReference);
                }

                var documentsUpdate = new InteractionEvents.CollectionDocumentChange(ReferenceNodeChangeAction.Add, newDocs, collection);

                return Result.Ok(documentsUpdate);
            }
            catch (Exception e)
            {
                var message = "Failed to import document from text content: " + e.Message;
                Logger.Warn(e, "Cannot process clipboard data.");
                ErrorInteraction(message, "Import Error");
                return Result.Fail<InteractionEvents.CollectionDocumentChange>(message);
            }
        }
        
        public Result<InteractionEvents.CollectionDocumentChange> CreateItem(CollectionReference collection)
        {
            if (collection is FileCollectionReference)
            {
                return AddFileToDatabase(collection.Database);
            }

            var newDoc = new BsonDocument
            {
                ["_id"] = ObjectId.NewObjectId()
            };

            var documentReference = collection.AddItem(newDoc);
            
            var documentsCreated = new InteractionEvents.CollectionDocumentChange(ReferenceNodeChangeAction.Add, documentReference, collection);

            return Result.Ok(documentsCreated);
        }

        public Result<bool> RevealInExplorer(DatabaseReference database)
        {
            var isOpen = _viewInteractionResolver.RevealInExplorer(database.Location);

            return Result.Ok(isOpen);
        }
        
        protected bool ConfirmInteraction(string message, string title = "Are you sure?")
        {
            return MessageBox.Show(
                message,
                title,
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            ) == MessageBoxResult.Yes;
        }

        protected void ErrorInteraction(string message, string title = "Database error")
        {
            MessageBox.Show(
                message,
                title,
                MessageBoxButton.OK,
                MessageBoxImage.Error
            );
        }

    }
}