using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Navigation;

namespace LiteDbExplorer.Controls
{
    /// <summary>
    /// Interaction logic for ExceptionViewer.xaml
    /// </summary>
    public partial class ExceptionViewer : UserControl
    {
        // Adapted from: http://www.codeproject.com/Tips/469452/WPF-ExceptionViewer
        public ExceptionViewer() :
            this(null, null)
        {
        }

        /// <summary>
        /// The exception and header message cannot be null.  If owner is specified, this window
        /// uses its Style and will appear centered on the Owner.  You can override this before
        /// calling ShowDialog().
        /// </summary>
        public ExceptionViewer(string headerMessage, Exception e)
        {
            InitializeComponent();

            Loaded += (sender, args) =>
            {
                treeCol.Width = new GridLength(treeCol.ActualWidth, GridUnitType.Pixel);
                _chromeWidth = ActualWidth - mainGrid.ActualWidth;
                ToggleDetails();
                CalcMaxTreeWidth();
            };

            if (DefaultPaneBrush != null)
            {
                treeView1.Background = DefaultPaneBrush;
            }

            docViewer.Background = treeView1.Background;

            // We use three font sizes.  The smallest is based on whatever the "standard"
            // size is for the current system/app, taken from an arbitrary control.

            _small = treeView1.FontSize;
            _med = _small * 1.1;
            _large = _small * 1.2;

            BuildTree(e, headerMessage);
        }

        public static readonly DependencyProperty ExceptionSourceProperty =
            DependencyProperty.Register(
                nameof(ExceptionSource), typeof(Exception), typeof(ExceptionViewer),
                new PropertyMetadata(default(Exception), OnExceptionSourceChanged));

        public Exception ExceptionSource
        {
            get => (Exception)GetValue(ExceptionSourceProperty);
            set => SetValue(ExceptionSourceProperty, value);
        }

        public static readonly RoutedEvent ExceptionSourceChangedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(ExceptionSourceChanged),
                RoutingStrategy.Bubble,
                typeof(RoutedPropertyChangedEventHandler<Exception>),
                typeof(ExceptionViewer));

        public event RoutedPropertyChangedEventHandler<Exception> ExceptionSourceChanged
        {
            add => AddHandler(ExceptionSourceChangedEvent, value);
            remove => RemoveHandler(ExceptionSourceChangedEvent, value);
        }

        private static void OnExceptionSourceChanged(
            DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var instance = (ExceptionViewer)d;
            var args = new RoutedPropertyChangedEventArgs<Exception>(
                (Exception)e.OldValue,
                (Exception)e.NewValue)
            {
                RoutedEvent = ExceptionViewer.ExceptionSourceChangedEvent
            };
            instance.RaiseEvent(args);
            instance.BuildTree(args.NewValue, String.Empty);
        }

        public static readonly DependencyProperty ControlsBorderBrushProperty = DependencyProperty.Register(
            nameof(ControlsBorderBrush), typeof(Brush), typeof(ExceptionViewer), new PropertyMetadata(default(Brush)));

        public Brush ControlsBorderBrush
        {
            get => (Brush)GetValue(ControlsBorderBrushProperty);
            set => SetValue(ControlsBorderBrushProperty, value);
        }

        public static readonly DependencyProperty ShowDetailsProperty = DependencyProperty.Register(
            nameof(ShowDetails), typeof(bool), typeof(ExceptionViewer), new PropertyMetadata(false, OnShowDetailPropertyChanged));
        
        public bool ShowDetails
        {
            get => (bool) GetValue(ShowDetailsProperty);
            set => SetValue(ShowDetailsProperty, value);
        }

        private static void OnShowDetailPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ExceptionViewer exceptionViewer)
            {
                exceptionViewer.ToggleDetails();
            }
        }

        private void ToggleDetails()
        {
            if (ShowDetails)
            {
                treeView1.Visibility = Visibility.Visible;
                gridSplitter.Visibility = Visibility.Visible;
                Grid.SetColumn(docViewer, 2);
                Grid.SetColumnSpan(docViewer, 0);
            }
            else
            {
                treeView1.Visibility = Visibility.Collapsed;
                gridSplitter.Visibility = Visibility.Collapsed;
                Grid.SetColumn(docViewer, 0);
                Grid.SetColumnSpan(docViewer, 3);
            }
        }

        /// <summary>
        /// The default title to use for the ExceptionViewer window.  Automatically initialized 
        /// to "Error - [ProductName]" where [ProductName] is taken from the application's
        /// AssemblyProduct attribute (set in the AssemblyInfo.cs file).  You can change this
        /// default, or ignore it and set Title yourself before calling ShowDialog().
        /// </summary>
        public static string DefaultTitle
        {
            get
            {
                if (_defaultTitle == null)
                {
                    if (string.IsNullOrEmpty(Product))
                    {
                        _defaultTitle = "Error";
                    }
                    else
                    {
                        _defaultTitle = "Error - " + Product;
                    }
                }

                return _defaultTitle;
            }
            set => _defaultTitle = value;
        }

        public static Brush DefaultPaneBrush
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the value of the AssemblyProduct attribute of the app.  
        /// If unable to lookup the attribute, returns an empty string.
        /// </summary>
        public static string Product => _product ?? (_product = GetProductName());

        static string _defaultTitle;
        static string _product;

        // Font sizes based on the "normal" size.
        readonly double _small;
        readonly double _med;
        readonly double _large;

        // This is used to dynamically calculate the mainGrid.MaxWidth when the Window is resized,
        // since I can't quite get the behavior I want without it.  See CalcMaxTreeWidth().
        double _chromeWidth;

        // Initializes the Product property.
        static string GetProductName()
        {
            var result = "";

            try
            {
                var appAssembly = GetAppAssembly();

                var customAttributes = appAssembly?.GetCustomAttributes(typeof(AssemblyProductAttribute), false);

                if (customAttributes != null && customAttributes.Length > 0)
                {
                    result = ((AssemblyProductAttribute)customAttributes[0]).Product;
                }
            }
            catch
            {
                // ignored
            }

            return result;
        }

        // Tries to get the assembly to extract the product name from.
        private static Assembly GetAppAssembly()
        {
            Assembly appAssembly = null;

            try
            {
                // This is supposedly how Windows.Forms.Application does it.
                appAssembly = Application.Current.MainWindow.GetType().Assembly;
            }
            catch
            {
                // ignored
            }

            // If the above didn't work, try less desireable ways to get an assembly.

            if (appAssembly == null)
            {
                appAssembly = Assembly.GetEntryAssembly();
            }

            return appAssembly ?? (appAssembly = Assembly.GetExecutingAssembly());
        }

        // Builds the tree in the left pane.
        // Each TreeViewItem.Tag will contain a list of Inlines
        // to display in the right-hand pane When it is selected.
        void BuildTree(Exception e, string summaryMessage)
        {
            docViewer.Document = null;
            treeView1.Items.Clear();

            // The first node in the tree contains the summary message and all the
            // nested exception messages.

            var inlines = new List<Inline>();
            var firstItem = new TreeViewItem { Header = "Summary" };
            treeView1.Items.Add(firstItem);

            if (!string.IsNullOrEmpty(summaryMessage))
            {
                var inline = new Bold { FontSize = _large };
                
                ReplaceWithLinks(inline, summaryMessage);

                inlines.Add(inline);
                inlines.Add(new LineBreak());
            }

            // Now add top-level nodes for each exception while building
            // the contents of the first node.
            while (e != null)
            {
                AddLines(inlines, e.Message);

                inlines.Add(new LineBreak());

                AddException(e);
                e = e.InnerException;
            }

            firstItem.Tag = inlines;
            firstItem.IsSelected = true;
        }

        private void ReplaceWithLinks(Span inline, string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var filePathRegex = new Regex(@"((?:[a-zA-Z]\:){0,1}(?:[\\/][\w.]+){1,})", RegexOptions.Compiled);
            var filePathsMatches = filePathRegex.Matches(message);
	
            if (filePathsMatches.Count > 0)
            {
                var parts = filePathRegex.Split(message);
                foreach (var part in parts)
                {
                    if (filePathRegex.IsMatch(part))
                    {
                        var hyperLink = new Hyperlink
                        {
                            NavigateUri = new Uri(part),
                            ToolTip = $"Open: {part}",
                            Inlines =
                            {
                                part
                            }
                        };
                        hyperLink.RequestNavigate += HyperLinkOnRequestNavigate;
                        inline.Inlines.Add(hyperLink);
                    }
                    else
                    {
                        inline.Inlines.Add(new Run(part));
                    }
                }
            }
            else
            {
                inline.Inlines.Add(message);
            }
        }

        private void HyperLinkOnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (e.Uri != null)
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            }
            e.Handled = true;
        }

        void AddProperty(List<Inline> inlines, string propName, object propVal)
        {
            inlines.Add(new LineBreak());
            var inline = new Bold(new Run(propName + ":")) { FontSize = _med };
            inlines.Add(inline);
            inlines.Add(new LineBreak());

            if (propVal is string str)
            {
                // Might have embedded newlines.

                AddLines(inlines, str);
            }
            else
            {
                inlines.Add(new Run(propVal.ToString()));
            }
            inlines.Add(new LineBreak());
        }

        // Adds the string to the list of Inlines, substituting
        // LineBreaks for an newline chars found.
        void AddLines(ICollection<Inline> inlines, string str)
        {
            var lines = str.Split('\n');

            inlines.Add(new Run(lines[0].Trim('\r')));

            foreach (var line in lines.Skip(1))
            {
                inlines.Add(new LineBreak());
                inlines.Add(new Run(line.Trim('\r')));
            }
        }

        // Adds the exception as a new top-level node to the tree with child nodes
        // for all the exception's properties.
        void AddException(Exception e)
        {
            // Create a list of Inlines containing all the properties of the exception object.
            // The three most important properties (message, type, and stack trace) go first.

            var exceptionItem = new TreeViewItem();
            var inlines = new List<Inline>();
            var properties = e.GetType().GetProperties();

            exceptionItem.Header = e.GetType();
            exceptionItem.Tag = inlines;
            treeView1.Items.Add(exceptionItem);

            Inline inline = new Bold(new Run(e.GetType().ToString()));
            inline.FontSize = _large;
            inlines.Add(inline);
            inlines.Add(new LineBreak());

            AddProperty(inlines, "Message", e.Message);
            AddProperty(inlines, "Stack Trace", e.StackTrace);

            foreach (var info in properties)
            {
                // Skip InnerException because it will get a whole
                // top-level node of its own.

                if (info.Name == "InnerException")
                {
                    continue;
                }

                var value = info.GetValue(e, null);

                if (value == null)
                {
                    continue;
                }

                if (value is string s)
                {
                    if (string.IsNullOrEmpty(s))
                    {
                        continue;
                    }
                }
                else if (value is IDictionary data)
                {
                    value = RenderDictionary(data);
                    if (string.IsNullOrEmpty((string) value))
                    {
                        continue;
                    }
                }
                else if (value is IEnumerable enumerable && !(enumerable is string))
                {
                    value = RenderEnumerable(enumerable);
                    if (string.IsNullOrEmpty((string) value))
                    {
                        continue;
                    }
                }

                if (info.Name != "Message" &&
                    info.Name != "StackTrace")
                {
                    // Add the property to list for the exceptionItem.
                    AddProperty(inlines, info.Name, value);
                }

                // Create a TreeViewItem for the individual property.
                var propertyItem = new TreeViewItem();
                var propertyInlines = new List<Inline>();

                propertyItem.Header = info.Name;
                propertyItem.Tag = propertyInlines;
                exceptionItem.Items.Add(propertyItem);
                AddProperty(propertyInlines, info.Name, value);
            }
        }

        static string RenderEnumerable(IEnumerable data)
        {
            var result = new StringBuilder();

            foreach (var obj in data)
            {
                result.AppendFormat("{0}\n", obj);
            }

            if (result.Length > 0) result.Length = result.Length - 1;
            return result.ToString();
        }

        static string RenderDictionary(IDictionary data)
        {
            var result = new StringBuilder();

            foreach (var key in data.Keys)
            {
                if (key != null && data[key] != null)
                {
                    result.AppendLine(key + " = " + data[key]);
                }
            }

            if (result.Length > 0) result.Length = result.Length - 1;
            return result.ToString();
        }

        private void treeView1_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ShowCurrentItem();
        }

        void ShowCurrentItem()
        {
            if (treeView1.SelectedItem != null)
            {
                var inlines = (treeView1.SelectedItem as TreeViewItem)?.Tag as IEnumerable<Inline>;
                var doc = new FlowDocument
                {
                    FontSize = _small,
                    FontFamily = treeView1.FontFamily,
                    TextAlignment = TextAlignment.Left,
                    Background = docViewer.Background
                };

                if (chkWrap.IsChecked == false)
                {
                    doc.PageWidth = CalcNoWrapWidth(inlines) + 50;
                }

                var para = new Paragraph();
                para.Inlines.AddRange(inlines);
                doc.Blocks.Add(para);
                docViewer.Document = doc;
            }
        }

        // Determines the page width for the Inlilness that causes no wrapping.
        double CalcNoWrapWidth(IEnumerable<Inline> inlines)
        {
            double pageWidth = 0;
            var tb = new TextBlock();
            var size = new Size(double.PositiveInfinity, double.PositiveInfinity);

            foreach (var inline in inlines)
            {
                tb.Inlines.Clear();
                tb.Inlines.Add(inline);
                tb.Measure(size);

                if (tb.DesiredSize.Width > pageWidth) pageWidth = tb.DesiredSize.Width;
            }

            return pageWidth;
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            // Build a FlowDocument with Inlines from all top-level tree items.

            var inlines = new List<Inline>();
            var doc = new FlowDocument();
            var para = new Paragraph();

            doc.FontSize = _small;
            doc.FontFamily = treeView1.FontFamily;
            doc.TextAlignment = TextAlignment.Left;

            foreach (TreeViewItem treeItem in treeView1.Items)
            {
                if (inlines.Any())
                {
                    // Put a line of underscores between each exception.

                    inlines.Add(new LineBreak());
                    inlines.Add(new Run("____________________________________________________"));
                    inlines.Add(new LineBreak());
                }

                if (treeItem.Tag != null && treeItem.Tag is IEnumerable<Inline> tagInlines)
                {
                    inlines.AddRange(tagInlines);
                }
            }

            para.Inlines.AddRange(inlines);
            doc.Blocks.Add(para);

            // Now place the doc contents on the clipboard in both
            // rich text and plain text format.

            var range = new TextRange(doc.ContentStart, doc.ContentEnd);
            var data = new DataObject();

            using (Stream stream = new MemoryStream())
            {
                range.Save(stream, DataFormats.Rtf);
                data.SetData(DataFormats.Rtf, Encoding.UTF8.GetString((stream as MemoryStream).ToArray()));
            }

            data.SetData(DataFormats.StringFormat, range.Text);
            Clipboard.SetDataObject(data);

            // The Inlines that were being displayed are now in the temporary document we just built,
            // causing them to disappear from the viewer.  This puts them back.

            ShowCurrentItem();
        }


        private void chkWrap_Checked(object sender, RoutedEventArgs e)
        {
            ShowCurrentItem();
        }

        private void chkWrap_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowCurrentItem();
        }

        private void CalcMaxTreeWidth()
        {
            // This prevents the GridSplitter from being dragged beyond the right edge of the window.
            // Another way would be to use star sizing for all Grid columns including the left 
            // Grid column (i.e. treeCol), but that causes the width of that column to change when the
            // window's width changes, which I don't like.

            // mainGrid.MaxWidth = ActualWidth - _chromeWidth;
            treeCol.MaxWidth = mainGrid.MaxWidth - textCol.MinWidth;
        }
    }
}
