using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using LiteDbExplorer.Converters;
using LiteDB;
using Xceed.Wpf.Toolkit;

namespace LiteDbExplorer.Controls
{
    public enum BsonEditorExpandMode
    {
        Inline,
        Window
    }

    public class BsonValueEditor
    {
        public static FrameworkElement GetBsonValueEditor(
            BsonEditorExpandMode expandMode,
            string bindingPath, BsonValue bindingValue,
            object bindingSource, bool readOnly, string keyName)
        {
            var binding = new Binding
            {
                Path = new PropertyPath(bindingPath),
                Source = bindingSource,
                Mode = BindingMode.TwoWay,
                Converter = new BsonValueToNetValueConverter(),
                UpdateSourceTrigger = UpdateSourceTrigger.Explicit
            };

            if (bindingValue.IsArray)
            {
                var arrayValue = bindingValue as BsonArray;

                if (expandMode == BsonEditorExpandMode.Window)
                {
                    var button = new Button
                    {
                        Content = "Array",
                        Style = StyleKit.MaterialDesignEntryButtonStyle
                    };

                    button.Click += (s, a) =>
                    {
                        arrayValue = bindingValue as BsonArray;
                        var window = new Windows.ArrayViewer(arrayValue, readOnly)
                        {
                            Owner = Application.Current.MainWindow,
                            Height = 500
                        };

                        if (window.ShowDialog() == true)
                        {
                            arrayValue.Clear();
                            arrayValue.AddRange(window.EditedItems);
                        }
                    };

                    return button;
                }
                
                var contentView = new LazyContentView
                {
                    LoadButton =
                    {
                        Content = $"[Array] {arrayValue?.Count} {keyName}"
                    }
                };

                contentView.LoadButton.Click += (s, a) =>
                {
                    if (contentView.ContentLoaded) return;

                    arrayValue = bindingValue as BsonArray;
                    var control = new ArrayViewerControl(arrayValue, readOnly);
                    control.CloseRequested += (sender, args) => { contentView.Content = null; };
                    contentView.Content = control;
                };

                return contentView;
            }

            if (bindingValue.IsDocument)
            {
                if (expandMode == BsonEditorExpandMode.Window)
                {
                    var button = new Button
                    {
                        Content = "Document",
                        Style = StyleKit.MaterialDesignEntryButtonStyle
                    };

                    button.Click += (s, a) =>
                    {
                        var window = new Windows.DocumentViewer(bindingValue as BsonDocument, readOnly)
                        {
                            Owner = Application.Current.MainWindow,
                            Height = 500
                        };

                        window.ShowDialog();
                    };

                    return button;
                }
                
                var contentView = new LazyContentView
                {
                    LoadButton =
                    {
                        Content = $"[Document] {keyName}"
                    }
                };

                contentView.LoadButton.Click += (s, a) =>
                {
                    if (contentView.ContentLoaded) return;

                    var bsonDocument = bindingValue as BsonDocument;
                    var control = new DocumentViewerControl(bsonDocument, readOnly);
                    control.CloseRequested += (sender, args) => { contentView.Content = null; };

                    contentView.Content = control;
                };

                return contentView;
            }

            if (bindingValue.IsBoolean)
            {
                var check = new CheckBox
                {
                    IsEnabled = !readOnly,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                check.SetBinding(ToggleButton.IsCheckedProperty, binding);
                return check;
            }

            if (bindingValue.IsDateTime)
            {
                var datePicker = new DateTimePicker
                {
                    TextAlignment = TextAlignment.Left,
                    IsReadOnly = readOnly,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                datePicker.SetBinding(DateTimePicker.ValueProperty, binding);

                /*var datePicker = new DatePicker
                {
                    SelectedDateFormat = DatePickerFormat.Long,
                    IsEnabled = !readOnly
                };

                datePicker.SetBinding(DatePicker.SelectedDateProperty, binding);*/

                return datePicker;
            }

            if (bindingValue.IsDouble)
            {
                var numberEditor = new DoubleUpDown
                {
                    TextAlignment = TextAlignment.Left,
                    IsReadOnly = readOnly,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                numberEditor.SetBinding(DoubleUpDown.ValueProperty, binding);
                return numberEditor;
            }

            if (bindingValue.IsInt32)
            {
                var numberEditor = new IntegerUpDown
                {
                    TextAlignment = TextAlignment.Left,
                    IsReadOnly = readOnly,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                numberEditor.SetBinding(IntegerUpDown.ValueProperty, binding);

                /*var numberEditor = new SpinControl
                {
                    IsEnabled = !readOnly
                };

                numberEditor.SetBinding(SpinControl.ValueProperty, binding);*/

                return numberEditor;
            }

            if (bindingValue.IsInt64)
            {
                var numberEditor = new LongUpDown
                {
                    TextAlignment = TextAlignment.Left,
                    IsReadOnly = readOnly,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                numberEditor.SetBinding(LongUpDown.ValueProperty, binding);
                return numberEditor;
            }

            if (bindingValue.IsString)
            {
                var stringEditor = new TextBox
                {
                    IsReadOnly = readOnly,
                    AcceptsReturn = true,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                stringEditor.SetBinding(TextBox.TextProperty, binding);
                return stringEditor;
            }

            if (bindingValue.IsBinary)
            {
                var text = new TextBlock
                {
                    Text = "[Binary Data]",
                    VerticalAlignment = VerticalAlignment.Center,
                };

                return text;
            }

            if (bindingValue.IsObjectId)
            {
                var text = new TextBox
                {
                    Text = bindingValue.AsString,
                    IsReadOnly = true,
                    VerticalAlignment = VerticalAlignment.Center,
                };

                return text;
            }

            {
                var stringEditor = new TextBox
                {
                    VerticalAlignment = VerticalAlignment.Center,
                };
                stringEditor.SetBinding(TextBox.TextProperty, binding);
                return stringEditor;
            }
        }
    }
}