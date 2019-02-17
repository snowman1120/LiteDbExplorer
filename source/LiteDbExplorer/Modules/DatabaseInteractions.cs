using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows;
using CSharpFunctionalExtensions;
using LiteDbExplorer.Windows;
using LiteDB;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NLog;

namespace LiteDbExplorer.Modules
{
    public interface IDatabaseInteractions
    {
        Paths PathDefinitions { get; }
        void CreateAndOpenDatabase();
        void OpenDatabase();
        void OpenDatabase(string path);
        void OpenDatabases(IEnumerable<string> paths);
        Result AddFileToDatabase(DatabaseReference database);
        void RemoveSelectedDocuments();
        void AddCollectionToSelectedDatabase();
        void RenameSelectedCollection();
        void DropSelectedCollection();
        void ExportSelectedCollection();
        void ExportCollection(CollectionReference collectionReference);
        void ExportSelectedDocuments();
        void ExportDocuments(ICollection<DocumentReference> documents);
        void CopySelectedDocuments();
    }

    [Export(typeof(IDatabaseInteractions))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public class DatabaseInteractions : IDatabaseInteractions
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        public DatabaseInteractions()
        {
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

        public Result AddFileToDatabase(DatabaseReference database)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "All files|*.*",
                Multiselect = false
            };

            if (dialog.ShowDialog() != true)
            {
                return Result.Fail("FILE_OPEN_CANCELED");
            }

            try
            {
                if (InputBoxWindow.ShowDialog("New file id:", "Enter new file id", Path.GetFileName(dialog.FileName),
                        out string id) == true)
                {
                    var file = database.AddFile(id, dialog.FileName);
                    Store.Current.SelectCollection(database.Collections.First(a => a.Name == "_files"));
                    Store.Current.SelectDocument(file);

                    return Result.Ok();
                }
            }
            catch (Exception exc)
            {
                ErrorInteraction("Failed to upload file:" + Environment.NewLine + exc.Message);
            }


            return Result.Fail("FILE_OPEN_FAIL");
        }

        public void RemoveSelectedDocuments()
        {
            if (!ConfirmInteraction("Are you sure you want to remove items?"))
            {
                return;
            }

            var currentSelectedDocuments = Store.Current.SelectedDocuments.ToList();

            Store.Current.SelectedCollection.RemoveItems(currentSelectedDocuments);
        }

        public void AddCollectionToSelectedDatabase()
        {
            try
            {
                if (InputBoxWindow.ShowDialog("New collection name:", "Enter new collection name", "", out string name) == true)
                {
                    Store.Current.SelectedDatabase.AddCollection(name);
                }
            }
            catch (Exception exc)
            {
                ErrorInteraction("Failed to add new collection:" + Environment.NewLine + exc.Message);
            }
        }

        public void RenameSelectedCollection()
        {
            try
            {
                if (InputBoxWindow.ShowDialog("New name:", "Enter new collection name", Store.Current.SelectedCollection.Name, out string name) == true)
                {
                    Store.Current.SelectedDatabase.RenameCollection(Store.Current.SelectedCollection.Name, name);
                }
            }
            catch (Exception exc)
            {
                ErrorInteraction("Failed to rename collection:" + Environment.NewLine + exc.Message);
            }
        }

        public void DropSelectedCollection()
        {
            try
            {
                if (ConfirmInteraction($"Are you sure you want to drop collection \"{Store.Current.SelectedCollection.Name}\"?"))
                {
                    Store.Current.SelectedDatabase.DropCollection(Store.Current.SelectedCollection.Name);
                    Store.Current.ResetSelectedCollection();
                }

            }
            catch (Exception exc)
            {
                ErrorInteraction("Failed to drop collection:" + Environment.NewLine + exc.Message);
            }
        }

        public void ExportSelectedCollection()
        {
            ExportCollection(Store.Current.SelectedCollection);
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

        public void ExportSelectedDocuments()
        {
            ExportDocuments(Store.Current.SelectedDocuments);

            /*if (Store.Current.SelectedDocumentsCount == 0)
            {
                return;
            }
            
            var documentReference = Store.Current.SelectedDocuments.First();

            if (Store.Current.SelectedCollection is FileCollectionReference)
            {
                if (Store.Current.SelectedDocumentsCount == 1)
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
                        foreach (var file in Store.Current.SelectedDocuments)
                        {
                            var path = Path.Combine(dialog.FileName, $"{file.LiteDocument["_id"].AsString}-{file.LiteDocument["filename"].AsString}");
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
                    if (Store.Current.SelectedDocumentsCount == 1)
                    {
                        File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(documentReference.LiteDocument, true, false));
                    }
                    else
                    {
                        var data = new BsonArray(Store.Current.SelectedDocuments.Select(a => a.LiteDocument));
                        File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(data, true, false));
                    }
                }
            }*/
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

        public void CopySelectedDocuments()
        {
            var data = new BsonArray(Store.Current.SelectedDocuments.Select(a => a.LiteDocument));
            Clipboard.SetData(DataFormats.Text, JsonSerializer.Serialize(data, true, false));
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