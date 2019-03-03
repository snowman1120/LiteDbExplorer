using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Presentation;

namespace LiteDbExplorer.Modules.DbCollection
{
    [Export(typeof(CollectionExplorerViewModel))]
    [PartCreationPolicy (CreationPolicy.NonShared)]
    public class CollectionExplorerViewModel : Document<CollectionReference>
    {
        private DocumentReference _selectedDocument;
        private IList<DocumentReference> _selectedDocuments;

        public override string InstanceId => SelectedCollection?.InstanceId;

        public override void Init(CollectionReference value)
        {
            if (value == null)
            {
                TryClose(false);
                return;
            }

            DisplayName = value.Name;
            GroupDisplayName = value.Database.Name;

            SelectedCollection = value;

            IconContent = IconProvider.GetImageIcon(value is FileCollectionReference ? "/Images/file-table.png" : "/Images/table.png", new ImageIconOptions{ Height = 15 });
        }
        
        public CollectionReference SelectedCollection { get; private set; }

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
        
        protected override void OnActivate()
        {
            base.OnActivate();
            Store.Current.SelectDatabase(SelectedCollection.Database);
            Store.Current.SelectCollection(SelectedCollection);
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
    }
}
