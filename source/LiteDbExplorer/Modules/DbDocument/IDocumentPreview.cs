namespace LiteDbExplorer.Modules.DbDocument
{
    public interface IDocumentPreview
    {
        void ActivateDocument(DocumentReference document);
        bool HasDocument { get; }
    }
}