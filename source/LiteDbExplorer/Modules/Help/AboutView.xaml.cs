using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Controls;

namespace LiteDbExplorer.Modules.Help
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutView : UserControl
    {
        public AboutView()
        {
            InitializeComponent();

            InitViewer();
        }

        private void InitViewer()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "LiteDbExplorer.Modules.Help.AboutContent.md";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            using (var reader = new StreamReader(stream))
            {
                string result = reader.ReadToEnd();
                Viewer.Markdown = result;
            }
        }

        private void OpenHyperlink(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            Process.Start(e.Parameter.ToString());
        }
    }
}
