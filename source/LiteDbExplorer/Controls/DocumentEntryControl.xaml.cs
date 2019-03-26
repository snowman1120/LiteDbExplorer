using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using LiteDB;

namespace LiteDbExplorer.Controls
{
    public class DocumentFieldData
    {
        public string Name { get; set; }

        public FrameworkElement EditControl { get; set; }

        public DocumentFieldData(string name, FrameworkElement editControl)
        {
            Name = name;
            EditControl = editControl;
        }
    }

    /// <summary>
    ///     Interaction logic for DocumentViewerControl.xaml
    /// </summary>
    public partial class DocumentEntryControl : UserControl
    {
        public static readonly RoutedUICommand PreviousItem = new RoutedUICommand
        (
            "Previous Item",
            "PreviousItem",
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.PageUp)
            }
        );

        public static readonly RoutedUICommand NextItem = new RoutedUICommand
        (
            "Next Item",
            "NextItem",
            typeof(Commands),
            new InputGestureCollection
            {
                new KeyGesture(Key.PageDown)
            }
        );

        private BsonDocument _currentDocument;
        private ObservableCollection<DocumentFieldData> _customControls;
        private DocumentReference _documentReference;

        private bool _loaded = false;

        public DocumentEntryControl()
        {
            InitializeComponent();

            ListItems.Loaded += (sender, args) =>
            {
                if (_loaded)
                {
                    return;
                }

                InvalidateItemsSize();

                _loaded = true;
            };
        }

        private DocumentEntryControl(WindowController windowController) : this()
        {
            _windowController = windowController;
        }

        public DocumentEntryControl(BsonDocument document, bool readOnly, WindowController windowController = null) :
            this(windowController)
        {
            IsReadOnly = readOnly;

            _currentDocument = document;
            _customControls = new ObservableCollection<DocumentFieldData>();

            for (var i = 0; i < document.Keys.Count; i++)
            {
                var key = document.Keys.ElementAt(i);
                _customControls.Add(NewField(key, readOnly));
            }

            ListItems.ItemsSource = _customControls;

            ButtonNext.Visibility = Visibility.Collapsed;
            ButtonPrev.Visibility = Visibility.Collapsed;

            if (readOnly)
            {
                ButtonClose.Visibility = Visibility.Visible;
                ButtonOK.Visibility = Visibility.Collapsed;
                ButtonCancel.Visibility = Visibility.Collapsed;
                DropNewField.Visibility = Visibility.Collapsed;
            }
        }

        public static readonly DependencyProperty DocumentReferenceProperty = DependencyProperty.Register(
            nameof(DocumentReference), typeof(DocumentReference), typeof(DocumentEntryControl),
            new PropertyMetadata(default(DocumentReference), OnDocumentReferenceChanged));

        private static void OnDocumentReferenceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = d as DocumentEntryControl;
            var documentReference = e.NewValue as DocumentReference;
            control?.LoadDocument(documentReference);
        }

        public DocumentReference DocumentReference
        {
            get => (DocumentReference) GetValue(DocumentReferenceProperty);
            set => SetValue(DocumentReferenceProperty, value);
        }

        public DocumentEntryControl(DocumentReference document, WindowController windowController = null) : this(
            windowController)
        {
            LoadDocument(document);
        }

        public bool IsReadOnly { get; }

        public bool DialogResult { get; set; }

        public void LoadDocument(DocumentReference document)
        {
            if (document.Collection is FileCollectionReference reference)
            {
                var fileInfo = reference.GetFileObject(document);
                GroupFile.Visibility = Visibility.Visible;
                FileView.LoadFile(fileInfo);
            }

            _currentDocument = document.Collection.LiteCollection.FindById(document.LiteDocument["_id"]);
            _documentReference = document;
            _customControls = new ObservableCollection<DocumentFieldData>();

            for (var i = 0; i < document.LiteDocument.Keys.Count; i++)
            {
                var key = document.LiteDocument.Keys.ElementAt(i);
                _customControls.Add(NewField(key, IsReadOnly));
            }

            ListItems.ItemsSource = _customControls;
        }

        private DocumentFieldData NewField(string key, bool readOnly)
        {
            var expandMode = OpenEditorMode.Inline;
            if (_windowController != null)
            {
                expandMode = OpenEditorMode.Window;
            }

            var valueEdit =
                BsonValueEditor.GetBsonValueEditor(
                    openMode: expandMode,
                    bindingPath: $"[{key}]",
                    bindingValue: _currentDocument[key],
                    bindingSource: _currentDocument,
                    readOnly: readOnly,
                    keyName: key);

            return new DocumentFieldData(key, valueEdit);
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.Tag is string key)
            {
                var item = _customControls.First(a => a.Name == key);
                _customControls.Remove(item);
                _currentDocument.Remove(key);
            }
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event EventHandler CloseRequested;

        private void Close()
        {
            OnCloseRequested();

            _windowController?.Close(DialogResult);
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            // TODO make array and document types use this as well
            foreach (var ctrl in _customControls)
            {
                var control = ctrl.EditControl;
                var values = control.GetLocalValueEnumerator();
                while (values.MoveNext())
                {
                    var current = values.Current;
                    if (BindingOperations.IsDataBound(control, current.Property))
                    {
                        var binding = control.GetBindingExpression(current.Property);
                        if (binding.IsDirty)
                        {
                            binding.UpdateSource();
                        }
                    }
                }
            }

            if (_documentReference != null)
            {
                _documentReference.LiteDocument = _currentDocument;
                _documentReference.Collection.UpdateItem(_documentReference);
            }

            DialogResult = true;
            Close();
        }

        private async void NewFieldMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var maybeFieldName = await InputDialogView.Show("DocumentEntryDialogHost", "Enter name of new field.",
                "New field name");

            if (maybeFieldName.HasNoValue)
            {
                return;
            }

            var fieldName = maybeFieldName.Value.Trim();

            if (fieldName.Any(Char.IsWhiteSpace))
            {
                MessageBox.Show("Field name can not contain white spaces.", "", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            if (_currentDocument.Keys.Contains(fieldName))
            {
                MessageBox.Show($"Field \"{fieldName}\" already exists!", "", MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var menuItem = sender as MenuItem;
            BsonValue newValue;

            switch (menuItem.Header as string)
            {
                case "String":
                    newValue = new BsonValue(string.Empty);
                    break;
                case "Boolean":
                    newValue = new BsonValue(false);
                    break;
                case "Double":
                    newValue = new BsonValue((double) 0);
                    break;
                case "Decimal":
                    newValue = new BsonValue((decimal) 0.0m);
                    break;
                case "Int32":
                    newValue = new BsonValue(0);
                    break;
                case "Int64":
                    newValue = new BsonValue((long) 0);
                    break;
                case "DateTime":
                    newValue = new BsonValue(DateTime.MinValue);
                    break;
                case "Array":
                    newValue = new BsonArray();
                    break;
                case "Document":
                    newValue = new BsonDocument();
                    break;
                default:
                    throw new Exception("Uknown value type.");
            }

            _currentDocument.Add(fieldName, newValue);
            var newField = NewField(fieldName, false);
            _customControls.Add(newField);
            newField.EditControl.Focus();
            ItemsField_SizeChanged(ListItems, null);
            ListItems.ScrollIntoView(newField);
        }

        private bool _invalidatingSize;
        private readonly WindowController _windowController;

        private async void ItemsField_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_invalidatingSize)
            {
                return;
            }

            _invalidatingSize = true;

            var listView = sender as ListView;
            var grid = listView.View as GridView;
            var newWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth - 10 -
                           grid.Columns[0].ActualWidth - grid.Columns[2].ActualWidth;

            if (newWidth > 0)
            {
                grid.Columns[1].Width = Math.Max(140, newWidth);
            }

            if (_loaded)
            {
                await Task.Delay(50);
            }

            _invalidatingSize = false;
        }

        private void InvalidateItemsSize()
        {
            ItemsField_SizeChanged(ListItems, null);
        }

        private void NextItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_documentReference == null)
                e.CanExecute = false;
            else
            {
                var index = _documentReference.Collection.Items.IndexOf(_documentReference);
                e.CanExecute = index + 1 < _documentReference.Collection.Items.Count;
            }
        }

        private void NextItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var index = _documentReference.Collection.Items.IndexOf(_documentReference);

            if (index + 1 < _documentReference.Collection.Items.Count)
            {
                var newDocument = _documentReference.Collection.Items[index + 1];
                LoadDocument(newDocument);
            }
        }

        private void PreviousItemCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (_documentReference == null)
                e.CanExecute = false;
            else
            {
                var index = _documentReference.Collection.Items.IndexOf(_documentReference);
                e.CanExecute = index > 0;
            }
        }

        private void PreviousItemCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var index = _documentReference.Collection.Items.IndexOf(_documentReference);

            if (index > 0)
            {
                var newDocument = _documentReference.Collection.Items[index - 1];
                LoadDocument(newDocument);
            }
        }

        protected virtual void OnCloseRequested()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}