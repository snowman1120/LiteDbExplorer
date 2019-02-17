using System.ComponentModel.Composition;
using Caliburn.Micro;

namespace LiteDbExplorer.Modules.Main
{
    [Export(typeof(IShellStatusBar))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    public class ShellStatusBarViewModel : PropertyChangedBase, IShellStatusBar
    {
        
    }
}