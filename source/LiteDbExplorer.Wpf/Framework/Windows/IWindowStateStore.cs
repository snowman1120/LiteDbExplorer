using System.Collections.Generic;

namespace LiteDbExplorer.Wpf.Framework.Windows
{
    public interface IWindowStateStore
    {
        Dictionary<string, WindowPosition> WindowPositions { get; set; }
    }
}