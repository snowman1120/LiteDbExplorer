using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LiteDB;

namespace LiteDbExplorer.Controls
{
    public class ArrayUIItem
    {
        public string Name { get; set; }

        public FrameworkElement Control { get; set; }

        public BsonValue Value { get; set; }

        public int? Index { get; set; }
    }

    /// <summary>
    /// Interaction logic for ArrayViewerControl.xaml
    /// </summary>
    public partial class ArrayViewerControl : UserControl
    {
        private readonly WindowController _windowController;

        public ObservableCollection<ArrayUIItem> Items
        {
            get; set;
        }

        public BsonArray EditedItems;

        public bool IsReadOnly { get; } = false;

        public bool DialogResult { get; set; }

        public event EventHandler CloseRequested;

        public ArrayViewerControl(BsonArray array, bool readOnly, WindowController windowController = null)
        {
            _windowController = windowController;

            InitializeComponent();
            
            IsReadOnly = readOnly;

            Items = new ObservableCollection<ArrayUIItem>();

            var index = 0;
            foreach (BsonValue item in array)
            {
                Items.Add(NewItem(item, index));
                index++;
            }

            ItemsItems.ItemsSource = Items;

            if (readOnly)
            {
                ButtonClose.Visibility = Visibility.Visible;
                ButtonOK.Visibility = Visibility.Collapsed;
                ButtonCancel.Visibility = Visibility.Collapsed;
                ButtonAddItem.Visibility = Visibility.Collapsed;
            }

            if (_windowController != null)
            {
                ScrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            }
        }
        
        private void ButtonRemove_Click(object sender, RoutedEventArgs e)
        {
            var value = (sender as Control).Tag as BsonValue;
            Items.Remove(Items.First(a => a.Value == value));
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
        
        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            //TODO make array and document types use this as well
            foreach (var control in Items.Select(a => a.Control))
            {
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

            DialogResult = true;
            EditedItems = new BsonArray(Items.Select(a => a.Value));
            Close();
        }

        private void Close()
        {
            OnCloseRequested();

            _windowController?.Close(DialogResult);
        }

        public ArrayUIItem NewItem(BsonValue value, int? index)
        {
            var keyName = value.Type.ToString();
            var arrayItem = new ArrayUIItem
            {
                Name = $"{index}:{keyName}",
                Value = value,
                Index = index
            };

            var expandMode = OpenEditorMode.Inline;
            if (_windowController != null)
            {
                expandMode = OpenEditorMode.Window;
            }

            var valueEdit = BsonValueEditor.GetBsonValueEditor(
                openMode: expandMode, 
                bindingPath: "Value", 
                bindingValue: value, 
                bindingSource: arrayItem, 
                readOnly: IsReadOnly, 
                keyName: keyName);

            arrayItem.Control = valueEdit;

            return arrayItem;
        }

        private void NewFieldMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            BsonValue newValue;
            ButtonAddItem.IsPopupOpen = false;

            switch (menuItem.Header as string)
            {
                case "String":
                    newValue = new BsonValue(string.Empty);
                    break;
                case "Boolean":
                    newValue = new BsonValue(false);
                    break;
                case "Double":
                    newValue = new BsonValue((double)0);
                    break;
                case "Decimal":
                    newValue = new BsonValue((decimal) 0.0m);
                    break;
                case "Int32":
                    newValue = new BsonValue((int)0);
                    break;
                case "Int64":
                    newValue = new BsonValue((long)0);
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

            var newItem = NewItem(newValue, Items.Count);
            Items.Add(newItem);
            newItem.Control.Focus();
            newItem.Control.BringIntoView();
        }

        protected virtual void OnCloseRequested()
        {
            CloseRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
