using System;
using System.Windows.Input;
using Caliburn.Micro;

namespace LiteDbExplorer.Wpf.Framework
{
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