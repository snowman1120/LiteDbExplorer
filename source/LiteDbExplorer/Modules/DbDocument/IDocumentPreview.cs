using Caliburn.Micro;

namespace LiteDbExplorer.Modules.DbDocument
{
    public interface IDocumentPreview : IScreen
    {
        void ActivateDocument(DocumentReference document);
        bool HasDocument { get; }
    }
}