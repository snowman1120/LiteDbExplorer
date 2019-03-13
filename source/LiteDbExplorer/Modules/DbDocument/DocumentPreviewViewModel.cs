using System.ComponentModel.Composition;
using Caliburn.Micro;
using JetBrains.Annotations;
using LiteDbExplorer.Core;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Modules.Main;
using LiteDB;
using MaterialDesignThemes.Wpf;

namespace LiteDbExplorer.Modules.DbDocument
{
    [Export(typeof(IDocumentPreview))]
    [Export(typeof(DocumentPreviewViewModel))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DocumentPreviewViewModel : Document<DocumentReference>, IDocumentPreview
    {
        public DocumentPreviewViewModel()
        {
            OpenAsDocumentCommand = new RelayCommand(OpenAsDocument, _ => CanOpenAsDocument);
        }
        
        public override string InstanceId => Document?.InstanceId;

        public override object IconContent => new PackIcon { Kind = PackIconKind.Json };

        public DocumentReference Document { get; private set; }

        public LiteFileInfo FileInfo { get; private set; }

        public bool IsDocumentView { get; private set; }

        public bool HasDocument => Document != null;

        public bool HasFileInfo => FileInfo != null;

        public bool HideFileInfo => FileInfo == null;

        public bool CanOpenAsDocument => Document != null && !IsDocumentView;

        public RelayCommand OpenAsDocumentCommand { get; set; }

        public override void Init(DocumentReference item)
        {
            IsDocumentView = true;
            ActivateDocument(item);
        }

        public void ActivateDocument(DocumentReference document)
        {
            DisplayName = "Document Preview";

            Document = document;

            if (Document != null)
            {
                DisplayName = Document.ToDisplayName();
                GroupDisplayName = Document.Collection?.Database.Name;
            }

            if (document != null && document.Collection is FileCollectionReference reference)
            {
                FileInfo = reference.GetFileObject(document);
            }
            else
            {
                FileInfo = null;
            }
        }

        [UsedImplicitly]
        public void OpenAsDocument(object _)
        {
            var documentSet = IoC.Get<IDocumentSet>();
            documentSet.OpenDocument<DocumentPreviewViewModel, DocumentReference>(Document);
        }
        
    }
}