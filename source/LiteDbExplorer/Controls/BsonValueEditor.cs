using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using LiteDbExplorer.Presentation.Converters;
using LiteDbExplorer.Windows;
using LiteDB;
using Xceed.Wpf.Toolkit;

namespace LiteDbExplorer.Controls
{
    public enum OpenEditorMode
    {
        Inline,
        Window
    }

    public class BsonValueEditor
    {
        // TODO: Infer Null value type to handle

        public static FrameworkElement GetBsonValueEditor(
            OpenEditorMode openMode,
            string bindingPath,
            BsonValue bindingValue,
            object bindingSource,
            bool readOnly,
            string keyName)
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

                if (openMode == OpenEditorMode.Window)
                {
                    var button = new Button
                    {
                        Content = $"[Array] {arrayValue?.Count} {keyName}",
                        Style = StyleKit.MaterialDesignEntryButtonStyle
                    };

                    button.Click += (s, a) =>
                    {
                        arrayValue = bindingValue as BsonArray;

                        var windowController = new WindowController {Title = "Array Editor"};
                        var control = new ArrayEntryControl(arrayValue, readOnly, windowController);
                        var window = new DialogWindow(control, windowController)
                        {
                            Owner = Application.Current.MainWindow,
                            Height = 600
                        };

                        if (window.ShowDialog() == true)
                        {
                            arrayValue?.Clear();
                            arrayValue?.AddRange(control.EditedItems);
                            
                            button.Content = $"[Array] {arrayValue?.Count} {keyName}";
                        }
                    };

                    return button;
                }

                var contentView = new ContentExpander
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
                    var control = new ArrayEntryControl(arrayValue, readOnly);
                    control.CloseRequested += (sender, args) => { contentView.Content = null; };
                    contentView.Content = control;
                };

                return contentView;
            }

            if (bindingValue.IsDocument)
            {
                var expandLabel = "[Document]";
                if (openMode == OpenEditorMode.Window)
                {
                    var button = new Button
                    {
                        Content = expandLabel,
                        Style = StyleKit.MaterialDesignEntryButtonStyle
                    };

                    button.Click += (s, a) =>
                    {
                        var windowController = new WindowController {Title = "Document Editor"};
                        var bsonDocument = bindingValue as BsonDocument;
                        var control = new DocumentEntryControl(bsonDocument, readOnly, windowController);
                        var window = new DialogWindow(control, windowController)
                        {
                            Owner = Application.Current.MainWindow,
                            Height = 600
                        };

                        window.ShowDialog();
                    };

                    return button;
                }

                var contentView = new ContentExpander
                {
                    LoadButton =
                    {
                        Content = expandLabel
                    }
                };

                contentView.LoadButton.Click += (s, a) =>
                {
                    if (contentView.ContentLoaded) return;

                    var bsonDocument = bindingValue as BsonDocument;
                    var control = new DocumentEntryControl(bsonDocument, readOnly);
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

            if (bindingValue.IsDecimal)
            {
                var numberEditor = new DecimalUpDown
                {
                    TextAlignment = TextAlignment.Left,
                    IsReadOnly = readOnly,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 0, 0)
                };

                numberEditor.SetBinding(DecimalUpDown.ValueProperty, binding);
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

            var defaultEditor = new TextBox
            {
                VerticalAlignment = VerticalAlignment.Center,
            };
            defaultEditor.SetBinding(TextBox.TextProperty, binding);
            return defaultEditor;
        }
    }
}