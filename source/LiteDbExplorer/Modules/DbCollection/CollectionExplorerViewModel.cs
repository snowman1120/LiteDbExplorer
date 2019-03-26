using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Security;
using System.Windows;
using Caliburn.Micro;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using LiteDbExplorer.Core;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Modules.DbDocument;
using MaterialDesignThemes.Wpf;

namespace LiteDbExplorer.Modules.DbCollection
{
    [Export(typeof(CollectionExplorerViewModel))]
    [PartCreationPolicy (CreationPolicy.NonShared)]
    public class CollectionExplorerViewModel : Document<CollectionReference>
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IViewInteractionResolver _viewInteractionResolver;
        private readonly IDatabaseInteractions _databaseInteractions;
        private DocumentReference _selectedDocument;
        private IList<DocumentReference> _selectedDocuments;
        private ICollectionReferenceListView _view;
        private bool _showDocumentPreview = true;
        private CollectionReference _collectionReference;
        
        [ImportingConstructor]
        public CollectionExplorerViewModel(
            IEventAggregator eventAggregator, 
            IViewInteractionResolver viewInteractionResolver,
            IDatabaseInteractions databaseInteractions)
        {
            _eventAggregator = eventAggregator;
            _viewInteractionResolver = viewInteractionResolver;
            _databaseInteractions = databaseInteractions;

            DocumentPreview = IoC.Get<IDocumentPreview>();
        }
        
        public override void Init(CollectionReference value)
        {
            if (value == null)
            {
                TryClose(false);
                return;
            }

            InstanceId = value.InstanceId;

            DisplayName = value.Name;

            if (value.Database != null)
            {
                GroupId = value.Database.InstanceId;
                GroupDisplayName = value.Database.Name;
            }
            
            IconContent = value is FileCollectionReference ? new PackIcon { Kind = PackIconKind.FileMultiple } : new PackIcon { Kind = PackIconKind.TableLarge, Height = 16 };
            

            CollectionReference = value;
        }

        [UsedImplicitly]
        public CollectionReference CollectionReference
        {
            get => _collectionReference;
            private set
            {
                if (_collectionReference != null)
                {
                    _collectionReference.ReferenceChanged -= OnCollectionReferenceChanged;
                    _collectionReference.DocumentsCollectionChanged -= OnDocumentsCollectionChanged;
                }
                _collectionReference = value;
                if (_collectionReference != null)
                {
                    _collectionReference.ReferenceChanged += OnCollectionReferenceChanged;
                    _collectionReference.DocumentsCollectionChanged += OnDocumentsCollectionChanged;
                }
            }
        }
        
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
        
        [UsedImplicitly]
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

        [UsedImplicitly]
        public string DocumentsCountInfo
        {
            get
            {
                if (CollectionReference?.Items == null)
                {
                    return "Collection is Null";
                }

                return CollectionReference.Items.Count == 1 ? "1 item" : $"{CollectionReference.Items.Count} items";
            }
        }

        [UsedImplicitly]
        public string SelectedDocumentsCountInfo 
        {
            get
            {
                if (SelectedDocuments == null)
                {
                    return string.Empty;
                }

                return SelectedDocuments.Count == 1 ? "1 selected item" : $"{SelectedDocuments.Count} selected items";
            }
        }

        protected override void OnViewLoaded(object view)
        {
            _view = view as ICollectionReferenceListView;
        }
        
        protected override void OnDeactivate(bool close)
        {
            Store.Current.SelectDocument(null);
            Store.Current.SelectedDocuments = null;
            
            if (close)
            {
                DocumentPreview?.TryClose();
                ShowDocumentPreview = false;
                SelectedDocuments = null;
                SelectedDocument = null;
                CollectionReference = null;
            }
        }
        
        

        public void ScrollIntoSelectedDocument()
        {
            _view?.ScrollIntoItem(SelectedDocument);
        }

        #region Handles

        private void OnCollectionReferenceChanged(object sender, ReferenceChangedEventArgs<CollectionReference> e)
        {
            switch (e.Action)
            {
                case ReferenceNodeChangeAction.Remove:
                    TryClose();
                    break;
                case ReferenceNodeChangeAction.Update:
                case ReferenceNodeChangeAction.Add:
                    _view?.UpdateView(e.Reference);
                    break;
            }
        }

        private void OnDocumentsCollectionChanged(object sender, CollectionReferenceChangedEventArgs<DocumentReference> e)
        {
            if (e.Action == ReferenceNodeChangeAction.Add)
            {
                SelectedDocument = e.Items.FirstOrDefault() ?? SelectedDocument;
                _view?.UpdateView(SelectedDocument);
            }

            if (e.Action == ReferenceNodeChangeAction.Update)
            {
                _view?.UpdateView(SelectedDocument);
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
                    _viewInteractionResolver.ActivateCollection(reference.CollectionReference, reference.Items);
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
            _databaseInteractions.OpenEditDocument(SelectedDocument);
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
