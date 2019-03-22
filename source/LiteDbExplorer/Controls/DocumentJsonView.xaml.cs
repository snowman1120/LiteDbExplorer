using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Indentation;
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
            
            _foldingManager = FoldingManager.Install(jsonEditor.TextArea);
            _foldingStrategy = new BraceFoldingStrategy();
            _searchReplacePanel = SearchReplacePanel.Install(jsonEditor);
            _searchReplacePanel.IsFindOnly = true;

            jsonEditor.TextArea.IndentationStrategy = new DefaultIndentationStrategy();

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
            jsonEditor.Document.Text = documentReference.LiteDocument.SerializeDecoded(true);

            _foldingStrategy.UpdateFoldings(_foldingManager, jsonEditor.Document);
        }

        private void ResetJson()
        {
            jsonEditor.Document.Text = string.Empty;
            _foldingStrategy.UpdateFoldings(_foldingManager, jsonEditor.Document);
        }
    }
}