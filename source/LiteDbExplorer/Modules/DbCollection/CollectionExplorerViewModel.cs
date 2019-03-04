using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using Caliburn.Micro;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Presentation;

namespace LiteDbExplorer.Modules.DbCollection
{
    [Export(typeof(CollectionExplorerViewModel))]
    [PartCreationPolicy (CreationPolicy.NonShared)]
    public class CollectionExplorerViewModel : Document<CollectionReference>, 
        IHandle<InteractionEvents.DocumentUpdated>, 
        IHandle<InteractionEvents.CollectionRemoved>,
        IHandle<InteractionEvents.CollectionDocumentsCreated>,
        IHandle<InteractionEvents.DatabaseClosed>
    {
        private DocumentReference _selectedDocument;
        private IList<DocumentReference> _selectedDocuments;
        private ICollectionListView _view;

        public override string InstanceId => CollectionReference?.InstanceId;

        [ImportingConstructor]
        public CollectionExplorerViewModel(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
        }

        public override void Init(CollectionReference value)
        {
            if (value == null)
            {
                TryClose(false);
                return;
            }

            DisplayName = value.Name;
            GroupDisplayName = value.Database.Name;

            CollectionReference = value;

            IconContent = IconProvider.GetImageIcon(value is FileCollectionReference ? "/Images/file-table.png" : "/Images/table.png", new ImageIconOptions{ Height = 15 });
        }
        
        public CollectionReference CollectionReference { get; private set; }

        public DocumentReference SelectedDocument
        {
            get => _selectedDocument;
            set
            {
                _selectedDocument = value;
                Store.Current.SelectDocument(_selectedDocument);
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public IList<DocumentReference> SelectedDocuments
        {
            get => _selectedDocuments;
            set
            {
                _selectedDocuments = value;
                Store.Current.SelectedDocuments = _selectedDocuments;
            }
        }

        public bool IsSearchOpen { get; private set; }

        protected override void OnViewLoaded(object view)
        {
            _view = view as ICollectionListView;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            Store.Current.SelectDatabase(CollectionReference.Database);
            Store.Current.SelectCollection(CollectionReference);
            CommandManager.InvalidateRequerySuggested();
        }

        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            Store.Current.SelectDatabase(null);
            Store.Current.SelectDocument(null);
            Store.Current.SelectedDocuments = null;
            Store.Current.ResetSelectedCollection();
            CommandManager.InvalidateRequerySuggested();
        }

        public void ScrollIntoSelectedDocument()
        {
            _view?.ScrollIntoItem(SelectedDocument);
        }

        public void Handle(InteractionEvents.DocumentUpdated message)
        {
            // TODO: Handle UpdateDocumentPreview();
            if (message.DocumentReference.BelongsToCollectionInstance(CollectionReference))
            {
                _view?.UpdateView(message.DocumentReference);
                _view?.ScrollIntoItem(message.DocumentReference);
            }
        }

        public void Handle(InteractionEvents.CollectionRemoved message)
        {
            if (message.CollectionReference.InstanceEquals(CollectionReference))
            {
                TryClose();
            }
        }

        public void Handle(InteractionEvents.DatabaseClosed message)
        {
            if (message.DatabaseReference.ContainsCollectionInstance(CollectionReference))
            {
                TryClose();
            }
        }

        public void Handle(InteractionEvents.CollectionDocumentsCreated message)
        {
            if (message.CollectionReference.InstanceEquals(CollectionReference))
            {
                _view?.UpdateView(CollectionReference);
            }
        }
    }
}
