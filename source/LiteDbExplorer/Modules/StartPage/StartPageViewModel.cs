using System.ComponentModel.Composition;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Presentation;

namespace LiteDbExplorer.Modules.StartPage
{
    [Export(typeof(StartPageViewModel))]
    [Export(typeof(IStartupDocument))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public class StartPageViewModel : Document, IStartupDocument
    {
        private readonly IDatabaseInteractions _databaseInteractions;
        
        [ImportingConstructor]
        public StartPageViewModel(IDatabaseInteractions databaseInteractions)
        {
            _databaseInteractions = databaseInteractions;

            PathDefinitions = databaseInteractions.PathDefinitions;
        }

        public override string DisplayName => "Start";

        public override object IconContent => IconProvider.GetImageIcon("/Images/icon.png", new ImageIconOptions{Height = 16});

        public Paths PathDefinitions { get; }

        public void OpenDatabase()
        {
            _databaseInteractions.OpenDatabase();
        }

        public void OpenRecentItem(RecentFileInfo recentFileInfo)
        {
            if (recentFileInfo == null)
            {
                return;
            }

            _databaseInteractions.OpenDatabase(recentFileInfo.FullPath);
        }
    }
}