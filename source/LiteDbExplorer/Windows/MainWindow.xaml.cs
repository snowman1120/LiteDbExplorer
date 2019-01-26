using LiteDB;
using LiteDbExplorer.Controls;
using LiteDbExplorer.Windows;
using Microsoft.Win32;
using NLog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LiteDbExplorer.Presentation;
using MahApps.Metro.Controls;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace LiteDbExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private PipeService _pipeService;
        private PipeServer _pipeServer;

        private readonly WindowPositionHandler _positionManager;
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        public Paths PathDefinitions
        {
            get; set;
        } = new Paths();
        
        public MainWindow()
        {
            InitializeComponent();

            _positionManager = new WindowPositionHandler(this, "Main");

            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2000);
                var update = new Update();

                try
                {
                    if (update.IsUpdateAvailable)
                    {
                        update.DownloadUpdate();

                        if (MessageBox.Show("New update found, install now?", "Update found", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            update.InstallUpdate();
                        }
                    }
                }
                catch (Exception exc)
                {
                    Logger.Error(exc, "Failed to process update.");
                }
            });

            DockSearch.Visibility = Visibility.Collapsed;

            Initialize();
        }

        private void Initialize()
        {
            Store.Current.SelectedDatabaseChange += (sender, args) =>
            {
                var databaseReference = args.Data;
                Title = databaseReference == null ? $"LiteDB Explorer {Versions.CurrentVersion}" : $"{databaseReference.Name} - LiteDB Explorer {Versions.CurrentVersion}";
            };

            Store.Current.SelectedCollectionChange += (sender, args) =>
            {
                BorderFilePreview.Visibility = Visibility.Collapsed;
            };

            Store.Current.SelectedDocumentChange += (sender, args) =>
            {
                var document = args.Data;
                
                BorderFilePreview.Visibility = Visibility.Collapsed;

                if (document != null && document.Collection is FileCollectionReference reference)
                {
                    var fileInfo = reference.GetFileObject(document);
                    FilePreview.LoadFile(fileInfo);
                    BorderFilePreview.Visibility = Visibility.Visible;
                }
            };
        }

        #region Exit Command

        private void ExitCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ExitCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Store.Current.CloseDatabases();

            if (Application.Current.MainWindow != null)
            {
                Application.Current.MainWindow.Close();
            }
        }
        #endregion Exit Command

        #region Open Command
        private void OpenCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void OpenCommand_Executed(object sender, ExecutedRoutedEventArgs e)
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
                MessageBox.Show("Failed to open database: " + exc.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion Open Command

        #region New Command
        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new SaveFileDialog()
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
        #endregion New Command

        #region Close Command
        private void CloseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.HasSelectedDatabase;
        }

        private void CloseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Store.Current.CloseSelectedDatabase();

        }
        #endregion Close Command

        #region EditDbProperties Command
        private void EditDbPropertiesCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.HasSelectedDatabase;
        }

        private void EditDbPropertiesCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var window = new DatabasePropertiesWindow(Store.Current.SelectedDatabase.LiteDatabase)
            {
                Owner = this
            };

            window.ShowDialog();
        }
        #endregion EditDbProperties Command

        #region AddFile Command
        private void AddFileCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.HasSelectedDatabase;
        }

        private void AddFileCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            AddFileToDatabase(Store.Current.SelectedDatabase);
        }

        #endregion AddFile Command

        #region Add Command
        private void AddCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.HasSelectedDatabase;
        }

        private void AddCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Store.Current.SelectedCollection is FileCollectionReference)
            {
                AddFileToDatabase(Store.Current.SelectedCollection.Database);
            }
            else
            {
                var newDoc = new BsonDocument
                {
                    ["_id"] = ObjectId.NewObjectId()
                };

                var documentReference = Store.Current.SelectedCollection.AddItem(newDoc);
                Store.Current.SelectDocument(documentReference);
                CollectionListView.ScrollIntoSelectedItem();

                UpdateGridColumns(newDoc);
            }

            UpdateDocumentPreview();
        }
        #endregion Add Command

        #region Edit Command
        private void EditCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.SelectedDocumentsCount == 1; //DbItemsSelectedCount == 1;
        }

        private void EditCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = Store.Current.SelectedDocument;

            var windowController = new WindowController {Title = "Document Editor"};
            var control = new DocumentViewerControl(item, windowController);
            var window = new DialogWindow(control, windowController)
            {
                Owner = Application.Current.MainWindow,
                Height = 600
            };

            /*var window = new Windows.DocumentViewer(item)
            {
                Owner = this
            };*/

            if (window.ShowDialog() == true)
            {
                UpdateGridColumns(item.LiteDocument);
                UpdateDocumentPreview();
            }
        }
        #endregion Edit Command

        #region Remove Command
        private void RemoveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.SelectedDocumentsCount > 0; //DbItemsSelectedCount > 0;
        }

        private void RemoveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MessageBox.Show(
                "Are you sure you want to remove items?",
                "Are you sure?",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            ) != MessageBoxResult.Yes)
            {
                return;
            }

            var currentSelectedDocuments = Store.Current.SelectedDocuments.ToList();
            Store.Current.SelectedCollection.RemoveItems(currentSelectedDocuments);
        }
        #endregion Remove Command

        #region Export Command
        private void ExportCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.SelectedDocumentsCount > 0; // DbItemsSelectedCount > 0;
        }

        private void ExportCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (Store.Current.SelectedDocumentsCount == 0)
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
                        (file.Collection as FileCollectionReference).SaveFile(file, dialog.FileName);
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

                            (file.Collection as FileCollectionReference).SaveFile(file, path);
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
            }
        }
        #endregion Export Command

        #region Refresh Collection Command
        private void RefreshCollectionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.HasSelectedCollection;
        }

        private void RefreshCollectionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Store.Current.SelectedCollection?.Refresh();
        }

        #endregion Refresh Collection Command

        #region AddCollection Command
        private void AddCollectionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.HasSelectedDatabase;
        }

        private void AddCollectionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (InputBoxWindow.ShowDialog("New collection name:", "Enter new colletion name", "", out string name) == true)
                {
                    Store.Current.SelectedDatabase.AddCollection(name);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(
                    "Failed to add new collection:" + Environment.NewLine + exc.Message,
                    "Database error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        #endregion AddCollection Command

        #region RenameCollection Command
        private void RenameCollectionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.SelectedCollection != null && Store.Current.SelectedCollection.Name != "_files" && Store.Current.SelectedCollection.Name != "_chunks";
        }

        private void RenameCollectionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (InputBoxWindow.ShowDialog("New name:", "Enter new colletion name", Store.Current.SelectedCollection.Name, out string name) == true)
                {
                    Store.Current.SelectedDatabase.RenameCollection(Store.Current.SelectedCollection.Name, name);
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(
                    "Failed to rename collection:" + Environment.NewLine + exc.Message,
                    "Database error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        #endregion RenameCollection Command

        #region DropCollection Command
        private void DropCollectionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.SelectedCollection != null && Store.Current.SelectedCollection.Name != "_files" && Store.Current.SelectedCollection.Name != "_chunks";
        }

        private void DropCollectionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (MessageBox.Show(
                    string.Format("Are you sure you want to drop collection \"{0}\"?", Store.Current.SelectedCollection.Name),
                    "Are you sure?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question
                ) == MessageBoxResult.Yes)
                {
                    Store.Current.SelectedDatabase.DropCollection(Store.Current.SelectedCollection.Name);
                    Store.Current.ResetSelectedCollection();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(
                    "Failed to drop collection:" + Environment.NewLine + exc.Message,
                    "Database error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
        #endregion DropCollection Command

        #region Refresh Database Command
        private void RefreshDatabaseCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.HasSelectedDatabase;
        }

        private void RefreshDatabaseCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Store.Current.ResetSelectedCollection();
            Store.Current.SelectedDatabase.Refresh();
        }
        #endregion Refresh Database Command

        #region Find Command
        private void FindCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.HasSelectedCollection;
        }

        private void FindCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DockSearch.Visibility = Visibility.Visible;
            TextSearch.Focus();
            TextSearch.SelectAll();
        }
        #endregion Find Command

        #region FindNext Command
        private void FindNextCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.HasSelectedCollection;
        }

        private void FindNextCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextSearch.Text))
            {
                return;
            }

            var skipIndex = -1;
            
            if (Store.Current.SelectedDocumentsCount > 0)
            {
                skipIndex = Store.Current.SelectedCollection.Items.IndexOf(Store.Current.SelectedDocuments.Last());
            }

            foreach (var item in Store.Current.SelectedCollection.Items.Skip(skipIndex + 1))
            {
                if (ItemMatchesSearch(TextSearch.Text, item, CheckSearchCase.IsChecked ?? false))
                {
                    SelectDocumentInView(item);
                    return;
                }
            }
            
            MainSnackbar.MessageQueue.Enqueue($"No results for '{TextSearch.Text}'.");
        }
        #endregion FindNext Command

        #region FindPrevious Command
        private void FindPreviousCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.HasSelectedCollection;
        }

        private void FindPreviousCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextSearch.Text))
            {
                return;
            }

            var skipIndex = 0;
            if (Store.Current.SelectedDocumentsCount > 0)
            {
                skipIndex = Store.Current.SelectedCollection.Items.Count - Store.Current.SelectedCollection.Items.IndexOf(Store.Current.SelectedDocuments.Last()) - 1;
            }

            foreach (var item in Store.Current.SelectedCollection.Items.Reverse().Skip(skipIndex + 1))
            {
                if (ItemMatchesSearch(TextSearch.Text, item, (bool)CheckSearchCase.IsChecked))
                {
                    SelectDocumentInView(item);
                    return;
                }
            }
        }
        #endregion FindPrevious Command

        #region Copy Command
        private void CopyCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.SelectedDocumentsCount > 0 && Store.Current.SelectedCollection != null && Store.Current.SelectedCollection.Name != "_files";
        }

        private void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var data = new BsonArray(Store.Current.SelectedDocuments.Select(a => a.LiteDocument));
            Clipboard.SetData(DataFormats.Text, JsonSerializer.Serialize(data, true, false));
        }
        #endregion Copy Command

        #region Paste Command
        private void PasteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.SelectedCollection != null && Store.Current.SelectedCollection.Name != "_files" && Clipboard.ContainsText();
        }

        private void PasteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                var textData = Clipboard.GetText();
                if (string.IsNullOrWhiteSpace(textData))
                {
                    return;
                }

                var newValue = JsonSerializer.Deserialize(textData);
                if (newValue.IsArray)
                {
                    foreach (var value in newValue.AsArray)
                    {
                        var doc = value.AsDocument;
                        Store.Current.SelectedCollection.AddItem(doc);
                        UpdateGridColumns(doc);
                    }
                }
                else
                {
                    var doc = newValue.AsDocument;
                    Store.Current.SelectedCollection.AddItem(doc);
                    UpdateGridColumns(doc);
                }

                UpdateDocumentPreview();
            }
            catch (Exception exc)
            {
                Logger.Warn(exc, "Cannot process clipboard data.");
                MessageBox.Show("Failed to paste document from clipboard: " + exc.Message, "Import Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        #endregion Paste Command

        private bool ItemMatchesSearch(string matchTerm, DocumentReference document, bool matchCase)
        {
            var stringData = JsonSerializer.Serialize(document.LiteDocument);
            var stringComparison =
                matchCase ? StringComparison.InvariantCulture : StringComparison.InvariantCultureIgnoreCase;

            return stringData.IndexOf(matchTerm, 0, stringComparison) != -1;
        }

        private void SelectDocumentInView(DocumentReference document)
        {
            Store.Current.SelectDocument(document);
            CollectionListView.ScrollIntoSelectedItem();
        }

        private void UpdateDocumentPreview()
        {
            // TODO: Update only changed node and keep tree state
            /*var document = DbSelectedItems.FirstOrDefault();
            if (document != null)
            {
                DocumentTreeView.ItemsSource = new DocumentTreeItemsSource(document);

                if (document.Collection is FileCollectionReference reference)
                {
                    var fileInfo = reference.GetFileObject(document);
                    FilePreview.LoadFile(fileInfo);
                    BorderFilePreview.Visibility = Visibility.Visible;
                }
                else
                {
                    BorderFilePreview.Visibility = Visibility.Collapsed;
                }
            }*/
        }

        private void UpdateGridColumns(BsonDocument dbItem)
        {
            CollectionListView.UpdateGridColumns(dbItem);
        }
        
        private void Window_Closed(object sender, EventArgs e)
        {
            Store.Current.CloseDatabases();

            Application.Current.Shutdown(0);
        }

        private void TreeDatabasese_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            Store.Current.SelectNode(e.NewValue);
        }
        
        private void RecentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var path = (sender as FrameworkElement)?.Tag as string;

            OpenDatabase(path);
        }

        private void RecentListItem_Click(object sender, MouseButtonEventArgs e)
        {
            var path = (sender as FrameworkElement)?.Tag as string;

            OpenDatabase(path);
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
                e.Handled = true;
            }
        }

        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
            {
                source = VisualTreeHelper.GetParent(source);
            }

            return source as TreeViewItem;
        }

        public void OpenDatabase(string path)
        {
            if (Store.Current.IsDatabaseOpen(path))
            {
                return;
            }

            if (!File.Exists(path))
            {
                MessageBox.Show(
                    "Cannot open database, file not found.",
                    "File not found",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            string password = null;
            if (DatabaseReference.IsDbPasswordProtected(path))
            {
                if (InputBoxWindow.ShowDialog("Database is password protected, enter password:", "Database password.", "", out password) != true)
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
                MessageBox.Show("Failed to open database:" + Environment.NewLine + e.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Logger.Error(e, "Failed to process update: ");
                return;
            }
        }

        private void IssueMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Config.IssuesUrl);
        }

        private void HomepageMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Config.HomepageUrl);
        }

        private void RecentItemMoreBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (ButtonOpen.ContextMenu != null)
            {
                ButtonOpen.ContextMenu.IsEnabled = true;
                ButtonOpen.ContextMenu.PlacementTarget = ButtonOpen;
                ButtonOpen.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                ButtonOpen.ContextMenu.IsOpen = true;
            }
        }

        private void ButtonCloseSearch_Click(object sender, RoutedEventArgs e)
        {
            DockSearch.Visibility = Visibility.Collapsed;
        }

        private void AddFileToDatabase(DatabaseReference database)
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
                if (InputBoxWindow.ShowDialog("New file id:", "Enter new file id", Path.GetFileName(dialog.FileName), out string id) == true)
                {
                    var file = database.AddFile(id, dialog.FileName);
                    Store.Current.SelectCollection(database.Collections.First(a => a.Name == "_files"));
                    Store.Current.SelectDocument(file);
                    CollectionListView.ScrollIntoSelectedItem();
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(
                    "Failed to upload file:" + Environment.NewLine + exc.Message,
                    "Database error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private void WindowMain_Loaded(object sender, RoutedEventArgs e)
        {
            SetColorTheme();

            Title = $"LiteDB Explorer {Versions.CurrentVersion}";

            _positionManager.RestoreSizeAndLocation(App.Settings);

            App.Settings.PropertyChanged += Settings_PropertyChanged;

            if ((Application.Current as App)?.OriginalInstance == true)
            {
                _pipeService = new PipeService();
                _pipeService.CommandExecuted += PipeService_CommandExecuted;
                _pipeServer = new PipeServer(Config.PipeEndpoint);
                _pipeServer.StartServer(_pipeService);

                var args = Environment.GetCommandLineArgs();
                if (args.Length > 1)
                {
                    PipeService_CommandExecuted(this, new CommandExecutedEventArgs(CmdlineCommands.Open, args[1]));
                }
            }
        }

        private void RestoreWindow()
        {
            Show();
            WindowState = WindowState.Normal;
            Activate();
            Focus();
        }

        private void PipeService_CommandExecuted(object sender, CommandExecutedEventArgs args)
        {
            Logger.Info(@"Executing command ""{0}"" from pipe with arguments ""{1}""", args.Command, args.Args);

            switch (args.Command)
            {
                case CmdlineCommands.Focus:
                    RestoreWindow();
                    break;

                case CmdlineCommands.Open:
                    OpenDatabase(args.Args);
                    RestoreWindow();
                    break;

                default:
                    Logger.Warn("Unknown command received");
                    break;
            }
        }

        private void WindowMain_LocationChanged(object sender, EventArgs e)
        {
            if (IsLoaded)
            {
                _positionManager.SavePosition(App.Settings);
            }
        }

        private void WindowMain_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (IsLoaded)
            {
                _positionManager.SaveSize(App.Settings);
            }
        }

        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.ColorTheme):
                    SetColorTheme();
                    break;
                case nameof(Settings.FieldSortOrder):
                    CollectionListView.UpdateGridColumns(Store.Current.SelectedCollection);
                    break;
            }
        }

        private void LeftPanel_Drop(object sender, DragEventArgs e)
        {
            try
            {
                if (e.Data.GetDataPresent(DataFormats.FileDrop) && e.Data.GetData(DataFormats.FileDrop, false) is string[] files)
                {
                    foreach (var path in files)
                    {
                        OpenDatabase(path);
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc, "Failed to open database: ");
                MessageBox.Show("Failed to open database: " + exc.Message, "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetColorTheme()
        {
            ThemeManager.SetColorTheme(App.Settings.ColorTheme);
            
            // Store.Current.ResetSelectedCollection();

        }

        private void ChangeThemeLabel_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ChangeThemeComboBox.IsDropDownOpen = !ChangeThemeComboBox.IsDropDownOpen;
        }

        
    }
}
