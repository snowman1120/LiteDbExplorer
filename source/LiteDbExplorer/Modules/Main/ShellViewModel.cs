using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Framework.Shell;

namespace LiteDbExplorer.Modules.Main
{
    [Export(typeof(IShell))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public sealed class ShellViewModel : Screen, IShell
    {
        public ShellViewModel()
        {
            DisplayName = "LiteDB Explorer";

            WindowMenu = IoC.Get<IShellMenu>();

            WindowRightMenu = IoC.Get<IShellRightMenu>();

            StatusBarContent = IoC.Get<IShellStatusBar>();

            LeftContent = IoC.Get<IDocumentExplorer>();

            MainContent = IoC.Get<IDocumentSet>();

            if (Properties.Settings.Default.ShowStartPageOnOpen)
            {
                MainContent.OpenDocument<IStartupDocument>();
            }

            MainContent.ActiveDocumentChanged += (sender, args) =>
            {
                if (!MainContent.Documents.Any() && Properties.Settings.Default.ShowStartOnCloseAll)
                {
                    MainContent.OpenDocument<IStartupDocument>();
                }
            };
        }
        
        public object WindowMenu { get; }

        public object WindowRightMenu { get; }

        public object StatusBarContent { get; set; }

        public object LeftContent { get; }

        public IDocumentSet MainContent { get; }
    }
}