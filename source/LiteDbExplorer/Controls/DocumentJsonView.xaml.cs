using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Indentation;
using ICSharpCode.AvalonEdit.Rendering;
using LiteDbExplorer.Controls.Editor;
using LiteDbExplorer.Controls.JsonViewer;
using LiteDbExplorer.Extensions;
using LiteDbExplorer.Presentation;

namespace LiteDbExplorer.Controls
{
    /// <summary>
    /// Interaction logic for DocumentJsonView.xaml
    /// </summary>
    public partial class DocumentJsonView : UserControl
    {
        readonly FoldingManager _foldingManager;
        readonly BraceFoldingStrategy _foldingStrategy;
        private readonly SearchReplacePanel _searchReplacePanel;

        public DocumentJsonView()
        {
            InitializeComponent();

            jsonEditor.ShowLineNumbers = true;
            jsonEditor.Encoding = Encoding.UTF8;
            
            jsonEditor.Options.EnableEmailHyperlinks = false;
            jsonEditor.Options.EnableHyperlinks = false;
            
            _foldingManager = FoldingManager.Install(jsonEditor.TextArea);
            _foldingStrategy = new BraceFoldingStrategy();
            _searchReplacePanel = SearchReplacePanel.Install(jsonEditor);
            _searchReplacePanel.IsFindOnly = true;

            jsonEditor.TextArea.MaxWidth = 1024;
            jsonEditor.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
            jsonEditor.TextArea.TextView.ElementGenerators.Add(new TruncateLongLines(1024));

            SetTheme();
            
            ThemeManager.CurrentThemeChanged += (sender, args) => { SetTheme(); };
        }

        public static readonly DependencyProperty DocumentSourceProperty = DependencyProperty.Register(
            nameof(DocumentSource),
            typeof(DocumentReference),
            typeof(DocumentJsonView),
            new PropertyMetadata(null, propertyChangedCallback: OnDocumentSourceChanged));

        public DocumentReference DocumentSource
        {
            get => (DocumentReference) GetValue(DocumentSourceProperty);
            set => SetValue(DocumentSourceProperty, value);
        }

        public void UpdateDocument()
        {
            if (DocumentSource != null)
            {
                SetJson(DocumentSource);
            }
            else
            {
                ResetJson();
            }
        }

        private void SetTheme()
        {
            // jsonEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("JavaScript");
            var resourceName = @"LiteDbExplorer.Controls.SyntaxDefinitions.Json.xshd";
            if (App.Settings.ColorTheme == ColorTheme.Dark)
            {
                jsonEditor.TextArea.Foreground = new SolidColorBrush(Colors.White);
                
                // _searchReplacePanel.MarkerBrush = new SolidColorBrush(Color.FromArgb(129, 206, 145, 120));
                _searchReplacePanel.MarkerBrush = new SolidColorBrush(Color.FromArgb(63, 144, 238, 144));
                jsonEditor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Color.FromRgb(206, 145, 120));
                resourceName = resourceName.Replace(@".xshd", @".dark.xshd");
            }
            else
            {
                _searchReplacePanel.MarkerBrush = new SolidColorBrush(Color.FromArgb(153, 144, 238, 144));
                jsonEditor.TextArea.TextView.LinkTextForegroundBrush = new SolidColorBrush(Color.FromRgb(26, 13, 171));
                jsonEditor.TextArea.Foreground = new SolidColorBrush(Colors.Black);
            }

            jsonEditor.SyntaxHighlighting = LoadHighlightingFromAssembly(resourceName);
        }

        private static IHighlightingDefinition LoadHighlightingFromAssembly(string name)
        {
            // https://edi.codeplex.com/SourceControl/latest#Edi/AvalonEdit/Highlighting/SQL.xshd
            using (var s = typeof(DocumentJsonView).Assembly.GetManifestResourceStream(name))
            {
                using (var reader = new XmlTextReader(s))
                {
                    return HighlightingLoader.Load(reader, HighlightingManager.Instance);
                }
            }
        }
        
        private static void OnDocumentSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is DocumentJsonView documentJsonView))
            {
                return;
            }

            if (e.NewValue is DocumentReference documentReference)
            {
                documentJsonView.SetJson(documentReference);
            }
            else
            {
                documentJsonView.ResetJson();
            }
        }

        private void SetJson(DocumentReference documentReference)
        {
            ThreadPool.QueueUserWorkItem(o => {
                var content = documentReference.LiteDocument.SerializeDecoded(true);
                // var doc = new TextDocument(content);
                // doc.SetOwnerThread(Application.Current.Dispatcher.Thread);
                
                
                Dispatcher.BeginInvoke((Action) (() =>
                {
                    jsonEditor.Document.Text = content;
                    _foldingStrategy.UpdateFoldings(_foldingManager, jsonEditor.Document);   

                }), DispatcherPriority.Normal);
            });
            
        }

        private void ResetJson()
        {
            jsonEditor.Document.Text = string.Empty;
            _foldingStrategy.UpdateFoldings(_foldingManager, jsonEditor.Document);
        }

        private class TruncateLongLines : VisualLineElementGenerator
        {
            private readonly int _maxLength;
            const string Ellipsis = " ... ";

            public TruncateLongLines(int? maxLength = null)
            {
                _maxLength = maxLength ?? 10000;
            }

            public override int GetFirstInterestedOffset(int startOffset)
            {
                var line = CurrentContext.VisualLine.LastDocumentLine;
                if (line.Length > _maxLength)
                {
                    var ellipsisOffset = line.Offset + _maxLength - Ellipsis.Length;
                    if (startOffset <= ellipsisOffset)
                    {
                        return ellipsisOffset;
                    }
                }
                return -1;
            }

            public override VisualLineElement ConstructElement(int offset)
            {
                var formattedTextElement = new FormattedTextElement(Ellipsis, CurrentContext.VisualLine.LastDocumentLine.EndOffset - offset)
                {
                    BackgroundBrush = new SolidColorBrush(Color.FromArgb(153, 238, 144, 144))
                };
                
                return formattedTextElement;
            }
        }
    }
}