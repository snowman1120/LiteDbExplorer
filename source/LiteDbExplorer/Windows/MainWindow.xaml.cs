using LiteDB;
using LiteDbExplorer.Controls;
using LiteDbExplorer.Windows;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using LiteDbExplorer.Modules;
using LiteDbExplorer.Presentation;
using MahApps.Metro.Controls;
using LogManager = NLog.LogManager;

namespace LiteDbExplorer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private PipeService _pipeService;
        private PipeServer _pipeServer;

        private readonly WindowPositionHandler _positionManager;
        private readonly DatabaseInteractions _databaseInteractions;
        private EventAggregator _eventAggregator;
        private MainWindowViewInteractionResolver _viewInteractionResolver;

        public Paths PathDefinitions
        {
            get; set;
        } = new Paths();
        
        public MainWindow()
        {
            InitializeComponent();

            _eventAggregator = new EventAggregator();

             _viewInteractionResolver = new MainWindowViewInteractionResolver{ Owner = this };

            _databaseInteractions = new DatabaseInteractions(_viewInteractionResolver);

            _positionManager = new WindowPositionHandler(this, "Main");
            
#if (!DEBUG)

            Task.Factory.StartNew(() =>
            {
                AppUpdateManager.Current.CheckForUpdates(false).ConfigureAwait(false);
            });
#endif

            AppUpdateManager.Current.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(AppUpdateManager.UpdateActionText) && AppUpdateManager.Current.UpdateActionText == "Restart")
                {
                    DropNewUpdatePanel.IsPopupOpen = true;
                }
            };

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
            _databaseInteractions.OpenDatabase();
        }
        #endregion Open Command

        #region New Command
        private void NewCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void NewCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _databaseInteractions.CreateAndOpenDatabase();
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
            _viewInteractionResolver.OpenDatabaseProperties(Store.Current.SelectedDatabase.LiteDatabase);
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
            var result = _databaseInteractions.CreateItem(Store.Current.SelectedCollection);
            if (result.IsSuccess)
            {
                var reference = result.Value;
                
                Store.Current.SelectCollection(reference.CollectionReference);
                Store.Current.SelectDocument(reference.NewDocuments?.FirstOrDefault());
                if (!reference.CollectionReference.IsFilesOrChunks)
                {
                    UpdateGridColumns(Store.Current.SelectedDocument.LiteDocument);
                }
                
                CollectionListView.ScrollIntoSelectedItem();

                UpdateDocumentPreview();
            }
        }
        #endregion Add Command

        #region Edit Command
        private void EditCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.SelectedDocumentsCount == 1;
        }

        private void EditCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var item = Store.Current.SelectedDocument;

            var document = _databaseInteractions.OpenEditDocument(item);
            if (document.HasValue)
            {
                UpdateGridColumns(document.Value.LiteDocument);
                UpdateDocumentPreview();
            }
        }
        #endregion Edit Command

        #region Remove Command
        private void RemoveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.SelectedDocumentsCount > 0;
        }

        private void RemoveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var currentSelectedDocuments = Store.Current.SelectedDocuments.ToList();
            _databaseInteractions.RemoveDocuments(currentSelectedDocuments);
        }
        #endregion Remove Command

        #region Export Command
        private void ExportCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.SelectedDocumentsCount > 0;
        }

        private void ExportCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _databaseInteractions.ExportDocuments(Store.Current.SelectedDocuments);
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
            _databaseInteractions.AddCollection(Store.Current.SelectedDatabase);
        }
        #endregion AddCollection Command

        #region RenameCollection Command
        private void RenameCollectionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.SelectedCollection != null && Store.Current.SelectedCollection.Name != "_files" && Store.Current.SelectedCollection.Name != "_chunks";
        }

        private void RenameCollectionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _databaseInteractions.RenameCollection(Store.Current.SelectedCollection);
        }

        #endregion RenameCollection Command

        #region DropCollection Command
        private void DropCollectionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.SelectedCollection != null && Store.Current.SelectedCollection.Name != "_files" && Store.Current.SelectedCollection.Name != "_chunks";
        }

        private void DropCollectionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var result = _databaseInteractions.DropCollection(Store.Current.SelectedCollection);
            if (result.IsSuccess)
            {
                Store.Current.ResetSelectedCollection();
            }

        }
        #endregion DropCollection Command

        #region ExportCollection Command
        private void ExportCollectionCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Store.Current.SelectedCollection != null && Store.Current.SelectedCollection.Name != "_files" && Store.Current.SelectedCollection.Name != "_chunks";
        }

        private void ExportCollectionCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _databaseInteractions.ExportCollection(Store.Current.SelectedCollection);
        }

        #endregion

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
                if (CheckSearchCase.IsChecked != null && ItemMatchesSearch(TextSearch.Text, item, (bool)CheckSearchCase.IsChecked))
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
            _databaseInteractions.CopyDocuments(Store.Current.SelectedDocuments);
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

                var result = _databaseInteractions.ImportDataFromText(Store.Current.SelectedCollection, textData);
                if (result.IsSuccess)
                {
                    UpdateDocumentPreview();
                }
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
            DocumentTreeView.UpdateDocument();
            DocumentJsonView.UpdateDocument();
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
            Store.Current.SelectNode(e.NewValue as IReferenceNode);
        }
        
        private void RecentMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var path = (sender as FrameworkElement)?.Tag as string;

            _databaseInteractions.OpenDatabase(path);
        }

        private void RecentListItem_Click(object sender, MouseButtonEventArgs e)
        {
            var path = (sender as FrameworkElement)?.Tag as string;
            
            _databaseInteractions.OpenDatabase(path);
        }

        private void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var treeViewItem = (e.OriginalSource as DependencyObject).VisualUpwardSearch();
            if (treeViewItem != null)
            {
                treeViewItem.IsSelected = true;
                e.Handled = true;
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
            var result = _databaseInteractions.AddFileToDatabase(database);
            if (result.IsSuccess)
            {
                var documentsCreated = result.Value;
                Store.Current.SelectCollection(documentsCreated.CollectionReference);
                Store.Current.SelectDocument(documentsCreated.NewDocuments?.FirstOrDefault());

                CollectionListView.ScrollIntoSelectedItem();
            }
        }

        private void WindowMain_Loaded(object sender, RoutedEventArgs e)
        {
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
                    _databaseInteractions.OpenDatabase(args.Args);
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
                    _databaseInteractions.OpenDatabases(files);
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


        private void UpdatePanelButtonOnClick(object sender, RoutedEventArgs e)
        {
            DropNewUpdatePanel.IsPopupOpen = true;
        }
    }
}
