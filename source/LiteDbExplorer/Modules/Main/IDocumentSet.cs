using System;
using Caliburn.Micro;
using LiteDbExplorer.Framework;

namespace LiteDbExplorer.Modules.Main
{
    public interface IDocumentSet : IScreen, IHaveActiveItem
    {
        Guid Id { get; }
        string ContentId { get; }
        Func<IDocument> NewDocumentFactory { get; }
        IObservableCollection<IDocument> Documents { get; }
    }
}