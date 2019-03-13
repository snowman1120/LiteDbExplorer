using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Security;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using LiteDbExplorer.Core;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Modules.DbDocument;
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
        private readonly IEventAggregator _eventAggregator;
        private readonly IViewInteractionResolver _viewInteractionResolver;
        private readonly IDatabaseInteractions _databaseInteractions;
        private DocumentReference _selectedDocument;
        private IList<DocumentReference> _selectedDocuments;
        private ICollectionListView _view;
        private bool _showDocumentPreview = true;

        public override string InstanceId => CollectionReference?.InstanceId;

        [ImportingConstructor]
        public CollectionExplorerViewModel(
            IEventAggregator eventAggregator, 
            IViewInteractionResolver viewInteractionResolver,
            IDatabaseInteractions databaseInteractions)
        {
            _eventAggregator = eventAggregator;
            _viewInteractionResolver = viewInteractionResolver;
            _databaseInteractions = databaseInteractions;
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

            DocumentPreview = IoC.Get<IDocumentPreview>();
        }
        
        [UsedImplicitly]
        public CollectionReference CollectionReference { get; private set; }

        [UsedImplicitly]
        public DocumentReference SelectedDocument
        {
            get => _selectedDocument;
            set
            {
                _selectedDocument = value;
                Store.Current.SelectDocument(_selectedDocument);
                if (_showDocumentPreview)
                {
                    DocumentPreview?.ActivateDocument(_selectedDocument);
                }
            }
        }

        [UsedImplicitly]
        public IList<DocumentReference> SelectedDocuments
        {
            get => _selectedDocuments;
            set
            {
                _selectedDocuments = value;
                Store.Current.SelectedDocuments = _selectedDocuments;
            }
        }

        public IDocumentPreview DocumentPreview { get; private set; }
        
        public bool IsSearchOpen { get; private set; }

        public bool ShowDocumentPreview
        {
            get => _showDocumentPreview;
            set
            {
                if (_showDocumentPreview == false && value)
                {
                    DocumentPreview?.ActivateDocument(_selectedDocument);
                }
                _showDocumentPreview = value;
            }
        }

        [UsedImplicitly]
        public bool HideDocumentPreview => SelectedDocument == null || !ShowDocumentPreview;

        protected override void OnViewLoaded(object view)
        {
            _view = view as ICollectionListView;
        }
        
        protected override void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);
            Store.Current.SelectDocument(null);
            Store.Current.SelectedDocuments = null;
        }
        
        public void ScrollIntoSelectedDocument()
        {
            _view?.ScrollIntoItem(SelectedDocument);
        }

        #region Handles

        public void Handle(InteractionEvents.DocumentUpdated message)
        {
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
                SelectedDocument = message.NewDocuments.FirstOrDefault();
            }
        }

        #endregion
        
        #region Routed Commands

        [UsedImplicitly]
        public void AddDocument()
        {
            _databaseInteractions.CreateItem(CollectionReference)
                .OnSuccess(reference =>
                {
                    _viewInteractionResolver.ActivateCollection(reference.CollectionReference, reference.NewDocuments);
                    _eventAggregator.PublishOnUIThread(reference);
                });
        }

        [UsedImplicitly]
        public bool CanAddDocument()
        {
            return CollectionReference != null;
        }

        [UsedImplicitly]
        public void EditDocument()
        {
            var maybe = _databaseInteractions.OpenEditDocument(SelectedDocument);
            if (maybe.HasValue)
            {
                maybe.Execute(value => _eventAggregator.PublishOnUIThread(new InteractionEvents.DocumentUpdated(value)));
            }
        }

        [UsedImplicitly]
        public bool CanEditDocument()
        {
            return SelectedDocument != null;
        }

        [UsedImplicitly]
        public void RemoveDocument()
        {
            _databaseInteractions.RemoveDocuments(SelectedDocuments);
        }

        [UsedImplicitly]
        public bool CanRemoveDocument()
        {
            return SelectedDocuments.HasAnyDocumentsReference();
        }

        [UsedImplicitly]
        public void ExportDocument()
        {
            _databaseInteractions.ExportDocuments(SelectedDocuments.ToList());
        }

        [UsedImplicitly]
        public bool CanExportDocument()
        {
            return SelectedDocuments.HasAnyDocumentsReference();
        }

        [UsedImplicitly]
        public void CopyDocument()
        {
            _databaseInteractions.CopyDocuments(SelectedDocuments);
        }

        [UsedImplicitly]
        public bool CanCopyDocument()
        {
            return SelectedDocuments.HasAnyDocumentsReference();
        }

        [UsedImplicitly]
        [SecurityCritical]
        public void PasteDocument()
        {
            var textData = Clipboard.GetText();

            _databaseInteractions
                .ImportDataFromText(CollectionReference, textData)
                .OnSuccess(update => _eventAggregator.PublishOnUIThread(update));
        }

        [UsedImplicitly]
        public bool CanPasteDocument()
        {
            return !CollectionReference.IsFilesCollection();
        }

        [UsedImplicitly]
        public void RefreshCollection()
        {
            CollectionReference?.Refresh();
        }

        [UsedImplicitly]
        public bool CanRefreshCollection()
        {
            return CollectionReference != null;
        }

        [UsedImplicitly]
        public void EditDbProperties()
        {
            _viewInteractionResolver.OpenDatabaseProperties(CollectionReference.Database);
        }

        [UsedImplicitly]
        public bool CanEditDbProperties()
        {
            return CollectionReference?.Database != null;
        }

        #endregion

    }
}
