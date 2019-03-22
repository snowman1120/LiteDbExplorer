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
        void OpenDocument(IDocument model);
        void CloseDocument(IDocument document);
        void ActivateItem(IDocument item);
        void DeactivateItem(IDocument item, bool close);
        void OpenDocument<TDoc>() where TDoc : IDocument;

        TDoc OpenDocument<TDoc, TNode>(TDoc model, TNode init)
            where TDoc : IDocument<TNode> where TNode : IReferenceNode;
        TDoc OpenDocument<TDoc, TNode>(TNode init) where TDoc : IDocument<TNode> where TNode : IReferenceNode;
        
        event EventHandler ActiveDocumentChanging;
        event EventHandler ActiveDocumentChanged;
    }
}