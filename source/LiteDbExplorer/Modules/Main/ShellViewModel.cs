using System;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Framework.Services;
using LiteDbExplorer.Modules.StartPage;

namespace LiteDbExplorer.Modules.Main
{
    [Export(typeof(IShell))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public sealed class ShellViewModel : Screen, IShell
    {
        public ShellViewModel()
        {
            DisplayName = $"LiteDB Explorer {Versions.CurrentVersion}";

            LeftWindowCommands = new ShellMenuViewModel();

            MainContent = new DocumentSetViewModel();
            
            MainContent.Documents.Add(MainContent.NewDocumentFactory());
        }
        
        public object LeftWindowCommands { get; }

        public IDocumentSet MainContent { get; }
    }
}