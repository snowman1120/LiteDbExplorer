using System.ComponentModel.Composition;
using Caliburn.Micro;

namespace LiteDbExplorer.Modules.DbDocument
{
    [Export(typeof(DocumentEntryViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DocumentEntryViewModel : Screen
    {
        public DocumentEntryViewModel()
        {
            DisplayName = "Document Editor";
        }

        public void Init(DocumentReference document)
        {
            Document = document;
        }

        public DocumentReference Document { get; private set; }

        protected override void OnDeactivate(bool close)
        {
            if (close)
            {
                Document = null;
            }
        }
    }
}