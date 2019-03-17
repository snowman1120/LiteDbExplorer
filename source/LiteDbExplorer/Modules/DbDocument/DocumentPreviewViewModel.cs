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
        private DocumentReference _document;
        private IDocumentDetailView _view;

        [ImportingConstructor]
        public DocumentPreviewViewModel(IEventAggregator eventAggregator)
        {
            OpenAsDocumentCommand = new RelayCommand(OpenAsDocument, _ => CanOpenAsDocument);

            eventAggregator.Subscribe(this);
        }
        
        public override object IconContent => new PackIcon { Kind = PackIconKind.Json, Height = 16 };

        public DocumentReference Document
        {
            get => _document;
            private set
            {
                if (_document != null)
                {
                    _document.ReferenceChanged -= OnDocumentReferenceChanged;
                }
                _document = value;
                if (_document != null)
                {
                    _document.ReferenceChanged += OnDocumentReferenceChanged;
                }

            }
        }
        
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
            InstanceId = document?.InstanceId;

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
            IoC.Get<IDocumentSet>().OpenDocument<DocumentPreviewViewModel, DocumentReference>(Document);
        }

        protected override void OnViewLoaded(object view)
        {
            _view = view as IDocumentDetailView;
        }

        protected override void OnDeactivate(bool close)
        {
            if (close && _document != null)
            {
                _document.ReferenceChanged -= OnDocumentReferenceChanged;
            }
        }

        #region Handles
        
        private void OnDocumentReferenceChanged(object sender, ReferenceChangedEventArgs<DocumentReference> e)
        {
            switch (e.Action)
            {
                case ReferenceNodeChangeAction.Remove:
                    TryClose();
                    break;
                case ReferenceNodeChangeAction.Update:
                    ActivateDocument(e.Reference);
                    _view?.UpdateView(e.Reference);
                    break;
            }
        }

        #endregion
    }
}