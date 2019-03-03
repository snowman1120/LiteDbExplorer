using System;
using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;

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
    
    public interface ILayoutItem : IScreen
    {
        Guid Id { get; }
        string ContentId { get; }
        ICommand CloseCommand { get; }
        // Uri IconSource { get; }
        bool IsSelected { get; set; }
        bool ShouldReopenOnStart { get; }
        // void LoadState(BinaryReader reader);
        // void SaveState(BinaryWriter writer);
    }
}