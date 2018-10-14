using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using LiteDB;

namespace LiteDbExplorer.Controls
{
    /// <summary>
    /// Interaction logic for ArrayViewerControl.xaml
    /// </summary>
    public partial class ArrayViewerControl : UserControl
    {
        public class ArrayUIItem
        {
            public FrameworkElement Control
            {
                get; set;
            }

            public BsonValue Value
            {
                get; set;
            }
        }

        public ObservableCollection<ArrayUIItem> Items
        {
            get; set;
        }

        public BsonArray EditedItems;

        private bool isReadOnly = false;
        public bool IsReadOnly
        {
            get
            {
                return isReadOnly;
            }
        }

        public bool DialogResult { get; set; }

        public event EventHandler CloseRequested;

        public ArrayViewerControl(BsonArray array, bool readOnly)
        {
            InitializeComponent();
            isReadOnly = readOnly;
            Items = new ObservableCollection<ArrayUIItem>();

            foreach (BsonValue item in array)
            {
                Items.Add(NewItem(item));
            }

            ItemsItems.ItemsSource = Items;

            if (readOnly)
            {
                ButtonClose.Visibility = Visibility.Visible;
                ButtonOK.Visibility = Visibility.Collapsed;
                ButtonCancel.Visibility = Visibility.Collapsed;
                ButtonAddItem.Visibility = Visibility.Collapsed;
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
            // TODO: Handle
            OnCloseRequested();
        }

        public ArrayUIItem NewItem(BsonValue value)
        {
            var arrayItem = new ArrayUIItem
            {
                Value = value
            };
            var keyName = value.Type.ToString();
            var valueEdit = BsonValueEditor.GetBsonValueEditor("Value", value, arrayItem, IsReadOnly, keyName);
            arrayItem.Control = valueEdit;
            return arrayItem;
        }

        private void NewFieldMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            BsonValue newValue;
            ButtonAddItem.IsOpen = false;

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

            var newItem = NewItem(newValue);
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
