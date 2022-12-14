using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
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
        private readonly IApplicationInteraction _applicationInteraction;
        private bool _showStartPageOnOpen;

        [ImportingConstructor]
        public StartPageViewModel(IDatabaseInteractions databaseInteractions, IApplicationInteraction applicationInteraction)
        {
            _databaseInteractions = databaseInteractions;
            _applicationInteraction = applicationInteraction;

            PathDefinitions = databaseInteractions.PathDefinitions;

            ShowStartPageOnOpen = Properties.Settings.Default.ShowStartPageOnOpen;
            
            PathDefinitions.RecentFiles.CollectionChanged += (sender, args) =>
            {
                NotifyOfPropertyChange(nameof(RecentFilesIsEmpty));
            };
        }
        
        public override string DisplayName => "Start";

        public override object IconContent => IconProvider.GetImageIcon("/Images/icon.png", new ImageIconOptions{Height = 16});

        public Paths PathDefinitions { get; }
        
        public bool ShowStartPageOnOpen
        {
            get => _showStartPageOnOpen;
            set
            {
                if (!Equals(_showStartPageOnOpen, value))
                {
                    _showStartPageOnOpen = value;
                    SaveSettings();
                }
            }
        }

        [UsedImplicitly]
        public bool RecentFilesIsEmpty => !PathDefinitions.RecentFiles.Any();

        public void SaveSettings()
        {
            Properties.Settings.Default.ShowStartPageOnOpen = ShowStartPageOnOpen;
            Properties.Settings.Default.Save();
        }
        
        [UsedImplicitly]
        public void OpenDatabase()
        {
            _databaseInteractions.OpenDatabase();
        }

        [UsedImplicitly]
        public void OpenRecentItem(RecentFileInfo recentFileInfo)
        {
            if (recentFileInfo == null)
            {
                return;
            }

            if (recentFileInfo.FileNotFound.HasValue && recentFileInfo.FileNotFound == true)
            {
                var message = $"File {recentFileInfo.FullPath} not found.\n\nRemove from list?";
                if (_applicationInteraction.ShowConfirm(message, "File not found!"))
                {
                    RemoveFromList(recentFileInfo);
                }
                return;
            }

            _databaseInteractions.OpenDatabase(recentFileInfo.FullPath);
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
        public void OpenDocs()
        {
            Process.Start("https://github.com/mbdavid/LiteDB/wiki");
        }

        [UsedImplicitly]
        public void RevealInExplorer(RecentFileInfo recentFileInfo)
        {
            if (recentFileInfo != null)
            {
                _applicationInteraction.RevealInExplorer(recentFileInfo.FullPath);
            }
        }

        [UsedImplicitly]
        public void CopyPath(RecentFileInfo recentFileInfo)
        {
            if (recentFileInfo != null)
            {
                _applicationInteraction.PutClipboardText(recentFileInfo.FullPath);
            }
        }

        [UsedImplicitly]
        public void RemoveFromList(RecentFileInfo recentFileInfo)
        {
            if (recentFileInfo != null)
            {
                PathDefinitions.RemoveRecentFile(recentFileInfo.FullPath);
            }
        }

        [UsedImplicitly]
        public void PinItem(RecentFileInfo recentFileInfo)
        {
            if (recentFileInfo != null)
            {
                PathDefinitions.SetRecentFileFixed(recentFileInfo.FullPath, true);
            }
        }

        [UsedImplicitly]
        public bool CanPinItem(RecentFileInfo recentFileInfo)
        {
            return recentFileInfo != null && !recentFileInfo.IsFixed;
        }

        [UsedImplicitly]
        public void UnPinItem(RecentFileInfo recentFileInfo)
        {
            if (recentFileInfo != null)
            {
                PathDefinitions.SetRecentFileFixed(recentFileInfo.FullPath, false);
            }
        }

        [UsedImplicitly]
        public bool CanUnPinItem(RecentFileInfo recentFileInfo)
        {
            return recentFileInfo != null && recentFileInfo.IsFixed;
        }
    }
}