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
        private readonly IViewInteractionResolver _viewInteractionResolver;

        [ImportingConstructor]
        public StartPageViewModel(IDatabaseInteractions databaseInteractions, IViewInteractionResolver viewInteractionResolver)
        {
            _databaseInteractions = databaseInteractions;
            _viewInteractionResolver = viewInteractionResolver;

            PathDefinitions = databaseInteractions.PathDefinitions;

            PathDefinitions.RecentFiles.CollectionChanged += (sender, args) =>
            {
                NotifyOfPropertyChange(nameof(RecentFilesIsEmpty));
            };
        }

        public override string DisplayName => "Start";

        public override object IconContent => IconProvider.GetImageIcon("/Images/icon.png", new ImageIconOptions{Height = 16});

        public Paths PathDefinitions { get; }

        [UsedImplicitly]
        public bool RecentFilesIsEmpty => !PathDefinitions.RecentFiles.Any();

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
                if (_viewInteractionResolver.ShowConfirm(message, "File not found!"))
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
                _viewInteractionResolver.RevealInExplorer(recentFileInfo.FullPath);
            }
        }

        [UsedImplicitly]
        public void CopyPath(RecentFileInfo recentFileInfo)
        {
            if (recentFileInfo != null)
            {
                _viewInteractionResolver.PutClipboardText(recentFileInfo.FullPath);
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