using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Indentation;
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

        public DocumentJsonView()
        {
            InitializeComponent();

            jsonEditor.ShowLineNumbers = true;
            jsonEditor.Encoding = Encoding.UTF8;

            SetSyntaxHighlightingTheme();

            _foldingManager = FoldingManager.Install(jsonEditor.TextArea);
            _foldingStrategy = new BraceFoldingStrategy();

            jsonEditor.TextArea.IndentationStrategy = new DefaultIndentationStrategy();

            ThemeManager.CurrentThemeChanged += (sender, args) => { SetSyntaxHighlightingTheme(); };
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

        private void SetSyntaxHighlightingTheme()
        {
            // jsonEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("JavaScript");
            var resourceName = @"LiteDbExplorer.Controls.SyntaxDefinitions.Json.xshd";
            if (App.Settings.ColorTheme == ColorTheme.Dark)
            {
                jsonEditor.TextArea.Foreground = new SolidColorBrush(Colors.White);
                resourceName = resourceName.Replace(@".xshd", @".dark.xshd");
            }
            else
            {
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