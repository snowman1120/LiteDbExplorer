using System;
using System.ComponentModel.Composition;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Presentation;

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

        public override object IconContent => IconProvider.GetImageIcon("/Images/icon.png", new ImageIconOptions{Height = 16});

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