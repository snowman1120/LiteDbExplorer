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
        void ExportCollection(CollectionReference collectionReference);
        void ExportDocuments(ICollection<DocumentReference> documents);
        Result<InteractionEvents.CollectionDocumentsCreated> AddFileToDatabase(DatabaseReference database);
        Result<InteractionEvents.CollectionDocumentsCreated> ImportDataFromText(CollectionReference collection, string textData);
        Result<InteractionEvents.CollectionDocumentsCreated> CreateItem(CollectionReference collection);
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
        private readonly IViewInteractionResolver _viewInteractionResolver;
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        [ImportingConstructor]
        public DatabaseInteractions(IViewInteractionResolver viewInteractionResolver)
        {
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

        public void CloseDatabase(DatabaseReference database)
        {
            Store.Current.CloseDatabase(database);
        }
        
        public Result<InteractionEvents.CollectionDocumentsCreated> AddFileToDatabase(DatabaseReference database)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "All files|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
            {
                return Result.Fail<InteractionEvents.CollectionDocumentsCreated>("FILE_OPEN_CANCELED");
            }

            try
            {
                if (InputBoxWindow.ShowDialog("New file id:", "Enter new file id", Path.GetFileName(dialog.FileName),
                        out string id) == true)
                {
                    var file = database.AddFile(id, dialog.FileName);

                    var documentsCreated = new InteractionEvents.CollectionDocumentsCreated(file.Collection, new [] {file});

                    return Result.Ok(documentsCreated);
                }
            }
            catch (Exception exc)
            {
                ErrorInteraction("Failed to upload file:" + Environment.NewLine + exc.Message);
            }


            return Result.Fail<InteractionEvents.CollectionDocumentsCreated>("FILE_OPEN_FAIL");
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

                    File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(data, true, false));
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
        
        public Maybe<DocumentReference> OpenEditDocument(DocumentReference document)
        {
            var result = _viewInteractionResolver.OpenEditDocument(document);
            if (result)
            {
                return document;
            }
            return null;
        }
        
        public Result<InteractionEvents.CollectionDocumentsCreated> ImportDataFromText(CollectionReference collection, string textData)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textData))
                {
                    return Result.Ok(new InteractionEvents.CollectionDocumentsCreated(collection, null));
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

                var documentsUpdate = new InteractionEvents.CollectionDocumentsCreated(collection, newDocs);

                return Result.Ok(documentsUpdate);
            }
            catch (Exception e)
            {
                var message = "Failed to import document from text content: " + e.Message;
                Logger.Warn(e, "Cannot process clipboard data.");
                ErrorInteraction(message, "Import Error");
                return Result.Fail<InteractionEvents.CollectionDocumentsCreated>(message);
            }
        }
        
        public Result<InteractionEvents.CollectionDocumentsCreated> CreateItem(CollectionReference collection)
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
            
            var documentsCreated = new InteractionEvents.CollectionDocumentsCreated(collection, new [] {documentReference});

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