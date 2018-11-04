using System.Windows;
using LiteDbExplorer.Framework;

namespace LiteDbExplorer.Modules.StartPage
{
    public class StartPageViewModel : Document
    {
        public override string DisplayName => "Start";

        public Paths PathDefinitions { get; set; } = new Paths();

        public void OpenRecentItem(string path)
        {
            MessageBox.Show($"Open '{path}'.");
        }
    }
}