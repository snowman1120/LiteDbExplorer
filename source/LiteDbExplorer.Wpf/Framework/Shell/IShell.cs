using Caliburn.Micro;

namespace LiteDbExplorer.Framework.Shell
{
    public interface IShell : IGuardClose, IDeactivate
    {
        // event EventHandler ActiveDocumentChanging;
        // event EventHandler ActiveDocumentChanged;

        // TODO: Rename this to ActiveItem.
        // ILayoutItem ActiveLayoutItem { get; set; }

        // TODO: Rename this to SelectedDocument.
        // IDocument ActiveItem { get; }

        // IObservableCollection<IDocument> Documents { get; }

        // void OpenDocument(IDocument model);

        // void CloseDocument(IDocument document);
    }
}