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

        string GroupId { get; set; }
        // IUndoRedoManager UndoRedoManager { get; }
    }

    public interface IStartupDocument : IDocument
    {

    }

    public interface IDocument<in T> : IReferenceNode, IDocument where T : IReferenceNode
    {
        void Init(T item);
    }
}