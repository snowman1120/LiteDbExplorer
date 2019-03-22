using System.IO;
using System.Windows.Media;
using LiteDbExplorer.Wpf.Framework;

namespace LiteDbExplorer.Framework
{
    public interface IDocument : ILayoutItem
    {
        string GroupDisplayName { get; set; }
        bool GroupDisplayNameIsVisible { get; set; }

        SolidColorBrush GroupDisplayBackground { get; set; }
        // IUndoRedoManager UndoRedoManager { get; }
    }

    public interface IDocument<in T> : IReferenceNode, IDocument where T : IReferenceNode
    {
        void Init(T item);
    }
}