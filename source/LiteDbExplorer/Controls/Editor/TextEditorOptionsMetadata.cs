using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Xml.Serialization;
using ICSharpCode.AvalonEdit;
using JetBrains.Annotations;
using PropertyTools.DataAnnotations;

namespace LiteDbExplorer.Controls.Editor
{
    [Serializable]
    [System.ComponentModel.DisplayName("Editor Options")]
    public partial class TextEditorOptionsMetadata : TextEditorOptions
    {
        private string _fontFamilyName = "Consolas";
        private double _fontSize = 12;
        private bool _showLineNumbers = true;

        public TextEditorOptionsMetadata()
        {
            HighlightCurrentLine = true;
            ConvertTabsToSpaces = true;
        }

        public TextEditorOptionsMetadata(TextEditorOptionsMetadata options)
            : base(options)
        {
            // get all the fields in the class
            var fields =
                typeof (TextEditorOptionsMetadata).GetFields(BindingFlags.NonPublic | BindingFlags.Public |
                                                             BindingFlags.Instance);

            // copy each value over to 'this'
            foreach (var fi in fields)
            {
                if (!fi.IsNotSerialized)
                    fi.SetValue(this, fi.GetValue(options));
            }
        }

        [System.ComponentModel.DisplayName("Font family")]
        [XmlIgnore]
        [FontPreview(14)]
        public FontFamily FontFamily
        {
            get => new FontFamily(FontFamilyName);
            set
            {
                if (value == null)
                {
                    FontFamilyName = "Consolas";
                    return;
                }
                FontFamilyName = value.Source;
            }
        }

        [DefaultValue("Consolas")]
        [XmlElement("FontFamily")]
        [System.ComponentModel.Browsable(false)]
        public string FontFamilyName
        {
            get => _fontFamilyName;
            set
            {
                if (value == _fontFamilyName) return;
                _fontFamilyName = value;
                OnPropertyChanged("FontFamily");
            }
        }

        [System.ComponentModel.DisplayName("Font size")]
        [DefaultValue(12)]
        [ItemsSourceProperty("FontSizes")]
        public double FontSize
        {
            get => _fontSize;
            set
            {
                if (value == _fontSize) return;
                _fontSize = value;
                OnPropertyChanged("FontSize");
            }
        }

        [System.ComponentModel.DisplayName("Show line numbers")]
        [DefaultValue(true)]
        public bool ShowLineNumbers
        {
            get => _showLineNumbers;
            set
            {
                if (value == _showLineNumbers) return;
                _showLineNumbers = value;
                OnPropertyChanged("ShowLineNumbers");
            }
        }

        [DefaultValue(true)]
        [System.ComponentModel.DisplayName("Highlight current line")]
        public override bool HighlightCurrentLine
        {
            get => base.HighlightCurrentLine;
            set => base.HighlightCurrentLine = value;
        }

        [DefaultValue(true)]
        [System.ComponentModel.DisplayName("Convert tabs to spaces")]
        public override bool ConvertTabsToSpaces
        {
            get => base.ConvertTabsToSpaces;
            set => base.ConvertTabsToSpaces = value;
        }

        [System.ComponentModel.DisplayName("Show spaces")]
        public override bool ShowSpaces
        {
            get => base.ShowSpaces;
            set => base.ShowSpaces = value;
        }

        [System.ComponentModel.DisplayName("Show tabs")]
        public override bool ShowTabs
        {
            get => base.ShowTabs;
            set => base.ShowTabs = value;
        }

        [System.ComponentModel.DisplayName("Show end of line")]
        public override bool ShowEndOfLine
        {
            get => base.ShowEndOfLine;
            set => base.ShowEndOfLine = value;
        }

        [System.ComponentModel.Browsable(false)]
        public override bool ShowBoxForControlCharacters
        {
            get => base.ShowBoxForControlCharacters;
            set => base.ShowBoxForControlCharacters = value;
        }

        [System.ComponentModel.Browsable(false)]
        public override bool EnableHyperlinks
        {
            get => base.EnableHyperlinks;
            set => base.EnableHyperlinks = value;
        }

        [System.ComponentModel.Browsable(false)]
        public override bool EnableEmailHyperlinks
        {
            get => base.EnableEmailHyperlinks;
            set => base.EnableEmailHyperlinks = value;
        }

        [System.ComponentModel.Browsable(false)]
        public override bool RequireControlModifierForHyperlinkClick
        {
            get => base.RequireControlModifierForHyperlinkClick;
            set => base.RequireControlModifierForHyperlinkClick = value;
        }

        [System.ComponentModel.DisplayName("Indentation size")]
        [Spinnable(1, 4, 1, 1000), Width(60)]
        public override int IndentationSize
        {
            get => base.IndentationSize;
            set => base.IndentationSize = value;
        }

        [System.ComponentModel.Browsable(false)]
        public override bool CutCopyWholeLine
        {
            get => base.CutCopyWholeLine;
            set => base.CutCopyWholeLine = value;
        }

        [System.ComponentModel.Browsable(false)]
        public override bool AllowScrollBelowDocument
        {
            get => base.AllowScrollBelowDocument;
            set => base.AllowScrollBelowDocument = value;
        }

        [System.ComponentModel.DisplayName("Word wrap indentation")]
        [Spinnable(1, 4, 0, 256), Width(60)]
        public override double WordWrapIndentation
        {
            get => base.WordWrapIndentation;
            set => base.WordWrapIndentation = value;
        }

        [System.ComponentModel.Browsable(false)]
        public override bool InheritWordWrapIndentation
        {
            get => base.InheritWordWrapIndentation;
            set => base.InheritWordWrapIndentation = value;
        }

        [System.ComponentModel.Browsable(false)]
        public override bool EnableVirtualSpace
        {
            get => base.EnableVirtualSpace;
            set => base.EnableVirtualSpace = value;
        }

        [System.ComponentModel.Browsable(false)]
        public override bool EnableImeSupport
        {
            get => base.EnableImeSupport;
            set => base.EnableImeSupport = value;
        }

        [System.ComponentModel.DisplayName("Show column ruler")]
        public override bool ShowColumnRuler
        {
            get => base.ShowColumnRuler;
            set => base.ShowColumnRuler = value;
        }
        
        [System.ComponentModel.DisplayName("Column ruler position")]
        [DefaultValue(80), Width(60)]
        public override int ColumnRulerPosition
        {
            get => base.ColumnRulerPosition;
            set => base.ColumnRulerPosition = value;
        }

        [System.ComponentModel.Browsable(false)]
        [DefaultValue(true)]
        public new bool EnableRectangularSelection
        {
            get => base.EnableRectangularSelection;
            set => base.EnableRectangularSelection = value;
        }

        [System.ComponentModel.Browsable(false)]
        [DefaultValue(true)]
        public new bool EnableTextDragDrop
        {
            get => base.EnableTextDragDrop;
            set => base.EnableTextDragDrop = value;
        }

        [System.ComponentModel.DisplayName("Hide cursor while typing")]
        [DefaultValue(true)]
        public new bool HideCursorWhileTyping
        {
            get => base.HideCursorWhileTyping;
            set => base.HideCursorWhileTyping = value;
        }

        [System.ComponentModel.Browsable(false)]
        [DefaultValue(false)]
        public new bool AllowToggleOverstrikeMode
        {
            get => base.AllowToggleOverstrikeMode;
            set => base.AllowToggleOverstrikeMode = value;
        }

        [System.ComponentModel.Browsable(false)]
        public List<double> FontSizes =>
            new List<double>
            {
                5.0,
                5.5,
                6.0,
                6.5,
                7.0,
                7.5,
                8.0,
                8.5,
                9.0,
                9.5,
                10.0,
                12.0,
                14.0,
                16.0,
                18.0,
                20.0,
                24.0
            };

        public TextEditorOptionsMetadata CopyFrom(TextEditorOptionsMetadata options)
        {
            var fields =
                typeof (TextEditorOptionsMetadata).GetProperties(BindingFlags.Instance | BindingFlags.Public |
                                                                 BindingFlags.SetProperty);
            foreach (var field in fields)
            {
                var value = field.GetValue(options);
                if (field.CanWrite)
                    field.SetValue(this, value);
            }
            return options;
        }

        public void SetBindings([NotNull] TextEditor textEditor)
        {
            if (textEditor == null) throw new ArgumentNullException("textEditor");

            textEditor.SetBinding(Control.FontFamilyProperty,
                new Binding("FontFamily") {Source = this, Mode = BindingMode.TwoWay});
            textEditor.SetBinding(Control.FontSizeProperty,
                new Binding("FontSize") {Source = this, Mode = BindingMode.TwoWay});
            textEditor.SetBinding(TextEditor.ShowLineNumbersProperty,
                new Binding("ShowLineNumbers") {Source = this, Mode = BindingMode.TwoWay});
            textEditor.SetBinding(TextEditor.OptionsProperty, new Binding {Source = this});
        }
    }
}