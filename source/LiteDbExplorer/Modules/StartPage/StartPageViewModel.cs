using System.ComponentModel.Composition;
using LiteDbExplorer.Framework;

namespace LiteDbExplorer.Modules.StartPage
{
    [Export(typeof(StartPageViewModel))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public class StartPageViewModel : Document
    {
        private readonly IDatabaseInteractions _databaseInteractions;
        
        [ImportingConstructor]
        public StartPageViewModel(IDatabaseInteractions databaseInteractions)
        {
            _databaseInteractions = databaseInteractions;

            PathDefinitions = databaseInteractions.PathDefinitions;
        }

        public override string DisplayName => "Start";

        public Paths PathDefinitions { get; }

        public void OpenDatabase()
        {
            _databaseInteractions.OpenDatabase();
        }

        public void OpenRecentItem(string path)
        {
            _databaseInteractions.OpenDatabase(path);
        }
    }
}