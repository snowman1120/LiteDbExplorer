using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
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
        void OpenDatabase(string path);
        void OpenDatabases(IEnumerable<string> paths);
        Result<TypedDocumentReference> AddFileToDatabase(DatabaseReference database);
        void ExportCollection(CollectionReference collectionReference);
        void ExportDocuments(ICollection<DocumentReference> documents);
        void OpenDatabaseProperties(LiteDatabase database);
        Result ImportDataFromText(CollectionReference collection, string textData);
        Result<TypedDocumentReference> CreateItem(CollectionReference collection);
        Result CopyDocuments(IEnumerable<DocumentReference> documents);
        Maybe<DocumentReference> OpenEditDocument(DocumentReference document);
        Result<bool> AddCollection(DatabaseReference database);
        Result<bool> RenameCollection(CollectionReference collection);
        Result<bool> DropCollection(CollectionReference collection);
        Result<bool> RemoveDocuments(IEnumerable<DocumentReference> documents);
    }

    [Export(typeof(IDatabaseInteractions))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public class DatabaseInteractions : IDatabaseInteractions
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IInteractionResolver _interactionResolver;
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        [ImportingConstructor]
        public DatabaseInteractions(IEventAggregator eventAggregator, IInteractionResolver interactionResolver)
        {
            _eventAggregator = eventAggregator;
            _interactionResolver = interactionResolver;
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

        public void OpenDatabase(string path)
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

            string password = null;
            if (DatabaseReference.IsDbPasswordProtected(path))
            {
                if (InputBoxWindow.ShowDialog("Database is password protected, enter password:", "Database password.",
                        "", out password) != true)
                {
                    return;
                }
            }

            if (PathDefinitions.RecentFiles.Contains(path))
            {
                PathDefinitions.RecentFiles.Remove(path);
            }

            PathDefinitions.RecentFiles.Insert(0, path);

            try
            {
                Store.Current.AddDatabase(new DatabaseReference(path, password));
            }
            catch (Exception e)
            {
                Logger.Error(e, "Failed to process update: ");
                ErrorInteraction("Failed to open database:" + Environment.NewLine + e.Message);
            }
        }

        public void OpenDatabases(IEnumerable<string> paths)
        {
            foreach (var path in paths)
            {
                OpenDatabase(path);
            }
        }
        
        public Result<TypedDocumentReference> AddFileToDatabase(DatabaseReference database)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "All files|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
            {
                return Result.Fail<TypedDocumentReference>("FILE_OPEN_CANCELED");
            }

            try
            {
                if (InputBoxWindow.ShowDialog("New file id:", "Enter new file id", Path.GetFileName(dialog.FileName),
                        out string id) == true)
                {
                    var file = database.AddFile(id, dialog.FileName);
                    
                    return Result.Ok(new TypedDocumentReference(DocumentType.File, file));
                }
            }
            catch (Exception exc)
            {
                ErrorInteraction("Failed to upload file:" + Environment.NewLine + exc.Message);
            }


            return Result.Fail<TypedDocumentReference>("FILE_OPEN_FAIL");
        }

        public Result<bool> RemoveDocuments(IEnumerable<DocumentReference> documents)
        {
            if (!ConfirmInteraction("Are you sure you want to remove items?"))
            {
                return Result.Ok(false);
            }

            Store.Current.SelectedCollection.RemoveItems(documents);

            return Result.Ok(true);
        }

        public Result<bool> AddCollection(DatabaseReference database)
        {
            try
            {
                if (InputBoxWindow.ShowDialog("New collection name:", "Enter new collection name", "", out string name) == true)
                {
                    database.AddCollection(name);

                    return Result.Ok(true);
                }

                return Result.Ok(false);
            }
            catch (Exception exc)
            {
                var message = "Failed to add new collection:" + Environment.NewLine + exc.Message;
                ErrorInteraction(message);
                return Result.Fail<bool>(message);
            }
        }

        public Result<bool> RenameCollection(CollectionReference collection)
        {
            try
            {
                var currentName = collection.Name;
                if (InputBoxWindow.ShowDialog("New name:", "Enter new collection name", currentName, out string name) == true)
                {
                    collection.Database.RenameCollection(currentName, name);
                    return Result.Ok(true);
                }

                return Result.Ok(false);
            }
            catch (Exception exc)
            {
                var message = "Failed to rename collection:" + Environment.NewLine + exc.Message;
                ErrorInteraction(message);
                return Result.Fail<bool>(message);
            }
        }

        public Result<bool> DropCollection(CollectionReference collection)
        {
            try
            {
                var collectionName = collection.Name;
                if (ConfirmInteraction($"Are you sure you want to drop collection \"{collectionName}\"?"))
                {
                    collection.Database.DropCollection(collectionName);
                    
                    return Result.Ok(true);
                }

                return Result.Ok(false);
            }
            catch (Exception exc)
            {
                var message = "Failed to drop collection:" + Environment.NewLine + exc.Message;
                ErrorInteraction(message);
                return Result.Fail<bool>(message);
            }
        }

        public void ExportCollection(CollectionReference collectionReference)
        {
            if (collectionReference == null)
            {
                return;
            }

            var dialog = new SaveFileDialog
            {
                Filter = "Json File|*.json",
                FileName = $"{collectionReference.Name}_export.json",
                OverwritePrompt = true
            };

            if (dialog.ShowDialog() == true)
            {
                var data = new BsonArray(collectionReference.LiteCollection.FindAll());

                File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(data, true, false));
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
                        File.WriteAllText(dialog.FileName,
                            JsonSerializer.Serialize(documentReference.LiteDocument, true, false));
                    }
                    else
                    {
                        var data = new BsonArray(documents.Select(a => a.LiteDocument));
                        File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(data, true, false));
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

        public void OpenDatabaseProperties(LiteDatabase database)
        {
            _interactionResolver.ShowDatabaseProperties(database);
        }

        public Maybe<DocumentReference> OpenEditDocument(DocumentReference document)
        {
            var result = _interactionResolver.OpenEditDocument(document);
            if (result)
            {
                return document;
            }
            return null;
        }
        
        public Result ImportDataFromText(CollectionReference collection, string textData)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textData))
                {
                    return Result.Ok();
                }

                var newValue = JsonSerializer.Deserialize(textData);
                var newDocs = new List<BsonDocument>();
                if (newValue.IsArray)
                {
                    foreach (var value in newValue.AsArray)
                    {
                        var doc = value.AsDocument;
                        collection.AddItem(doc);
                        newDocs.Add(doc);
                    }
                }
                else
                {
                    var doc = newValue.AsDocument;
                    collection.AddItem(doc);
                    newDocs.Add(doc);
                }

                _eventAggregator.BeginPublishOnUIThread(new InteractionEvents.DocumentsUpdated(nameof(ImportDataFromText), newDocs));
            }
            catch (Exception e)
            {
                var message = "Failed to import document from text content: " + e.Message;
                Logger.Warn(e, "Cannot process clipboard data.");
                ErrorInteraction(message, "Import Error");
                return Result.Fail(message);
            }

            return Result.Ok();
        }
        
        public Result<TypedDocumentReference> CreateItem(CollectionReference collection)
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

            Store.Current.SelectDocument(documentReference);

            _eventAggregator.BeginPublishOnUIThread(new InteractionEvents.DocumentsUpdated(nameof(CreateItem), newDoc));

            return Result.Ok(new TypedDocumentReference(DocumentType.BsonDocument, documentReference));
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