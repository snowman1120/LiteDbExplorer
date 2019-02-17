using System.ComponentModel.Composition;
using Caliburn.Micro;
using LiteDbExplorer.Framework.Services;
using LogManager = NLog.LogManager;

namespace LiteDbExplorer.Modules.Main
{
    [Export(typeof(IShell))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public sealed class ShellViewModel : Screen, IShell
    {
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        public ShellViewModel()
        {
            DisplayName = $"LiteDB Explorer {Versions.CurrentVersion}";
            
            WindowMenu = IoC.Get<IShellMenu>();

            StatusBarContent = IoC.Get<IShellStatusBar>();

            LeftContent = IoC.Get<IDocumentExplorer>();

            MainContent = IoC.Get<IDocumentSet>();
            
            MainContent.Documents.Add(MainContent.NewDocumentFactory());
            
        }
        
        public object WindowMenu { get; }

        public object StatusBarContent { get; set; }

        public object LeftContent { get; }
        
        public IDocumentSet MainContent { get; }
    }
}