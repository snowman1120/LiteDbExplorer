using System.Diagnostics;
using System.Windows;
using Caliburn.Micro;

namespace LiteDbExplorer.Modules.Main
{
    public class ShellMenuViewModel : PropertyChangedBase
    {
        public Paths PathDefinitions { get; set; } = new Paths();

        public void OpenRecentItem(string path)
        {
            MessageBox.Show($"Open '{path}'.");
        }

        public void OpenIssuePage()
        {
            Process.Start(Config.IssuesUrl);
        }

        public void OpenHomepage()
        {
            Process.Start(Config.HomepageUrl);
        }
    }
}