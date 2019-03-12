using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Dynamic;
using System.Linq;
using System.Windows;
using Caliburn.Micro;
using LiteDbExplorer.Controls;
using LiteDbExplorer.Modules.Database;
using LiteDbExplorer.Modules.DbCollection;
using LiteDbExplorer.Modules.Main;
using LiteDbExplorer.Windows;
using LiteDB;

namespace LiteDbExplorer.Modules
{
    [Export(typeof(IViewInteractionResolver))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class IoCViewInteractionResolver : IViewInteractionResolver
    {
        private readonly IWindowManager _windowManager;

        [ImportingConstructor]
        public IoCViewInteractionResolver(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public bool OpenDatabaseProperties(DatabaseReference database)
        {
            var vm = IoC.Get<IDatabasePropertiesView>();
            vm.Init(database);

            dynamic settings = new ExpandoObject();
            settings.Width = 480;
            settings.SizeToContent = SizeToContent.Height;
            settings.ResizeMode = ResizeMode.NoResize;

            return _windowManager.ShowDialog(vm, null, settings) == true;
        }

        public bool OpenEditDocument(DocumentReference document)
        {
            /*var vm = IoC.Get<DocumentEntryViewModel>();
            vm.Init(document);

            dynamic settings = new ExpandoObject();
            settings.Height = 600;
            settings.Width = 640;
            settings.SizeToContent = SizeToContent.Manual;

            return _windowManager.ShowDialog(vm, null, settings) == true;*/

            var windowController = new WindowController {Title = "Document Editor"};
            var control = new DocumentEntryControl(document, windowController);
            var window = new DialogWindow(control, windowController)
            {
                Height = 600
            };
            window.Owner = windowController.InferOwnerOf(window);

            return window.ShowDialog() == true;

            // TODO: Handle UpdateGridColumns(document.Value.LiteDocument) and UpdateDocumentPreview();
        }


        public bool RevealInExplorer(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
            {
                return false;
            }

            //Clean up file path so it can be navigated OK
            filePath = System.IO.Path.GetFullPath(filePath);
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{filePath}\"");

            return true;
        }

        public void ActivateCollection(CollectionReference collection, IEnumerable<DocumentReference> selectedDocuments = null)
        {
            if (collection == null)
            {
                return;
            }

            var documentSet = IoC.Get<IDocumentSet>();
            var vm = documentSet.OpenDocument<CollectionExplorerViewModel, CollectionReference>(collection);
            if (vm != null)
            {
                vm.SelectedDocument = selectedDocuments?.FirstOrDefault();
                vm.ScrollIntoSelectedDocument();
            }
        }

        public void ActivateDocument(DocumentReference document)
        {
            if (document == null)
            {
                return;
            }

            var documentSet = IoC.Get<IDocumentSet>();
            var vm = documentSet.OpenDocument<CollectionExplorerViewModel, CollectionReference>(document.Collection);
            if (vm != null)
            {
                vm.SelectedDocument = document;
                vm.ScrollIntoSelectedDocument();
            }
        }
    }
}