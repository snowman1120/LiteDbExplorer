using System;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using LiteDbExplorer.Framework.Shell;

namespace LiteDbExplorer.Modules.Main
{
    [Export(typeof(IShellStatusBar))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public class ShellStatusBarViewModel : PropertyChangedBase, IShellStatusBar
    {
        public ShellStatusBarViewModel()
        {
            CurrentVersion = Versions.CurrentVersion;
        }

        public Version CurrentVersion { get; }
    }
}