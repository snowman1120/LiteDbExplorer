using System.ComponentModel.Composition;
using System.Diagnostics;
using Caliburn.Micro;
using JetBrains.Annotations;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Framework.Shell;
using LiteDbExplorer.Modules.Help;
using LiteDbExplorer.Wpf.Modules.Settings;

namespace LiteDbExplorer.Modules.Main
{

    [Export(typeof(IShellMenu))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public class ShellMenuViewModel : PropertyChangedBase, IShellMenu
    {
        private readonly IDatabaseInteractions _databaseInteractions;
        private readonly IWindowManager _windowManager;
        private readonly IApplicationInteraction _applicationInteraction;

        [ImportingConstructor]
        public ShellMenuViewModel(
            IDatabaseInteractions databaseInteractions,
            IWindowManager windowManager,
            IApplicationInteraction applicationInteraction)
        {
            _databaseInteractions = databaseInteractions;
            _windowManager = windowManager;
            _applicationInteraction = applicationInteraction;

            PathDefinitions = databaseInteractions.PathDefinitions;
        }

        public Paths PathDefinitions { get; }
        
        [UsedImplicitly]
        public void OpenRecentItem(RecentFileInfo info)
        {
            if (info == null)
            {
                return;
            }

            _databaseInteractions.OpenDatabase(info.FullPath);
        }

        [UsedImplicitly]
        public void OpenIssuePage()
        {
            Process.Start(Config.IssuesUrl);
        }

        [UsedImplicitly]
        public void OpenHomepage()
        {
            Process.Start(Config.HomepageUrl);
        }

        [UsedImplicitly]
        public void OpenStartupDocument()
        {
            IoC.Get<IDocumentSet>().OpenDocument<IStartupDocument>();
        }

        [UsedImplicitly]
        public void OpenSettings()
        {
            _windowManager.ShowDialog(IoC.Get<SettingsViewModel>());
        }

        [UsedImplicitly]
        public void OpenAbout()
        {
            _applicationInteraction.ShowAbout();
        }

        [UsedImplicitly]
        public void ShowReleaseNotes()
        {
            _applicationInteraction.ShowReleaseNotes();
        }
    }
}