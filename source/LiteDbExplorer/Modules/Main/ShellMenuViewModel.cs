using System.ComponentModel.Composition;
using System.Diagnostics;
using Caliburn.Micro;
using LiteDbExplorer.Framework.Shell;

namespace LiteDbExplorer.Modules.Main
{

    [Export(typeof(IShellMenu))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public class ShellMenuViewModel : PropertyChangedBase, IShellMenu
    {
        private readonly IDatabaseInteractions _databaseInteractions;
        
        [ImportingConstructor]
        public ShellMenuViewModel(IDatabaseInteractions databaseInteractions)
        {
            _databaseInteractions = databaseInteractions;

            PathDefinitions = databaseInteractions.PathDefinitions;
        }

        public Paths PathDefinitions { get; }
        
        public void OpenRecentItem(string path)
        {
            _databaseInteractions.OpenDatabase(path);
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