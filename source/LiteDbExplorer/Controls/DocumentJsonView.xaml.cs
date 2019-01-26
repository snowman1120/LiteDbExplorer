using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using ICSharpCode.AvalonEdit.Indentation;
using LiteDbExplorer.Controls.JsonViewer;
using LiteDbExplorer.Presentation;
using LiteDB;

namespace LiteDbExplorer.Controls
{
    /// <summary>
    /// Interaction logic for DocumentJsonView.xaml
    /// </summary>
    public partial class DocumentJsonView : UserControl
    {
        FoldingManager foldingManager;
        BraceFoldingStrategy foldingStrategy;

        public DocumentJsonView()
        {
            InitializeComponent();

            jsonEditor.ShowLineNumbers = true;

            SetSyntaxHighlightingTheme();

            foldingManager = FoldingManager.Install(jsonEditor.TextArea);
            foldingStrategy = new BraceFoldingStrategy();
            jsonEditor.TextArea.IndentationStrategy = new DefaultIndentationStrategy();

            ThemeManager.CurrentThemeChanged += (sender, args) =>
            {
                SetSyntaxHighlightingTheme();
            };
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

        private void SetSyntaxHighlightingTheme()
        {
            // jsonEditor.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("JavaScript");
            var resourceName = @"LiteDbExplorer.Controls.SyntaxDefinitions.Json.xshd";
            if (App.Settings.ColorTheme == ColorTheme.Dark)
            {
                jsonEditor.TextArea.Foreground = new SolidColorBrush(Colors.White);
                resourceName = resourceName.Replace(".xshd", ".dark.xshd");
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
            var json = JsonSerializer.Serialize(documentReference.LiteDocument, true, false);
            jsonEditor.Document.Text = json;
            foldingStrategy.UpdateFoldings(foldingManager, jsonEditor.Document);
        }

        private void ResetJson()
        {
            jsonEditor.Document.Text = string.Empty;
            foldingStrategy.UpdateFoldings(foldingManager, jsonEditor.Document);
        }
    }
}
