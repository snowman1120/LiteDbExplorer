using Caliburn.Micro;

namespace LiteDbExplorer.Wpf.Framework
{
    public interface IWindow : IActivate, IDeactivate, INotifyPropertyChangedEx
    {
        
    }

    public abstract class WindowBase : Screen, IWindow
    {

    }
}