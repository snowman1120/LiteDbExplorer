using System.ComponentModel.Composition;
using System.Dynamic;
using System.Windows;
using Caliburn.Micro;
using LiteDbExplorer.Modules.Database;
using LiteDbExplorer.Modules.DbDocument;
using LiteDB;

namespace LiteDbExplorer.Modules
{
    [Export(typeof(IInteractionResolver))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class IoCInteractionResolver : IInteractionResolver
    {
        private readonly IWindowManager _windowManager;

        [ImportingConstructor]
        public IoCInteractionResolver(IWindowManager windowManager)
        {
            _windowManager = windowManager;
        }

        public bool ShowDatabaseProperties(LiteDatabase database)
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
            var vm = IoC.Get<DocumentEntryViewModel>();
            vm.Init(document);

            dynamic settings = new ExpandoObject();
            settings.Height = 600;
            settings.Width = 640;
            settings.SizeToContent = SizeToContent.Manual;

            return _windowManager.ShowDialog(vm, null, settings) == true;
        }
    }
}