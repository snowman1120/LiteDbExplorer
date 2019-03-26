using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Caliburn.Micro;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Framework.Services;
using LiteDbExplorer.Framework.Shell;
using LiteDbExplorer.Modules;

namespace LiteDbExplorer
{
    public class AppBootstrapper : BootstrapperBase
    {
        private CompositionContainer _container;

        public AppBootstrapper() : base(true)
        {
            Initialize();
        }

        
        protected override void Configure()
        {
            _container = new CompositionContainer(
                new AggregateCatalog(
                    catalogs: AssemblySource.Instance
                        .Select(x => new AssemblyCatalog(x)).OfType<ComposablePartCatalog>()
                )
            );

            var batch = new CompositionBatch();

            var windowManager = new AppWindowManager();
            windowManager.RegisterStateStore(Settings.Current);
            

            batch.AddExportedValue<IWindowManager>(windowManager);
            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            // batch.AddExportedValue<IInteractionResolver>()
            batch.AddExportedValue(_container);

            _container.Compose(batch);
            
            AddCustomConventions();
        }
        
        protected override object GetInstance(Type serviceType, string key)
        {
            var contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var exports = _container.GetExportedValues<object>(contract).ToArray();

            if (exports.Any())
            {
                return exports.First();
            }

            throw new Exception($"Could not locate any instances of contract {contract}.");
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override void BuildUp(object instance)
        {
            _container.SatisfyImportsOnce(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            DisplayRootViewFor<IShell>();
            RegisterApplicationCommandHandlers();

#if (!DEBUG)

            Task.Factory.StartNew(() =>
            {
                AppUpdateManager.Current.CheckForUpdates(false).ConfigureAwait(false);
            });
#endif

            var pipeServiceBootstrapper = _container.GetExportedValueOrDefault<PipeServiceBootstrapper>();
            
            pipeServiceBootstrapper?.Init();
        }

        private void RegisterApplicationCommandHandlers()
        {
            var handlers = _container.GetExportedValues<IApplicationCommandHandler>();
            handlers.SelectMany(p => p.CommandBindings)
                .ToList()
                .ForEach(binding =>
                {
                    CommandManager.RegisterClassCommandBinding(typeof(Window), binding);
                });
        }

        private void AddCustomConventions()
        {
            MessageBinder.SpecialValues.Add(@"$originalSourceContext", context =>
            {
                if (!(context.EventArgs is RoutedEventArgs args))
                {
                    return null;
                }

                if (!(args.OriginalSource is FrameworkElement fe))
                {
                    return null;
                }

                return fe.DataContext;
            });

            ConventionManager.AddElementConvention<MenuItem>(MenuItem.CommandProperty, nameof(MenuItem.CommandParameter), nameof(MenuItem.Click));
            ConventionManager.AddElementConvention<ButtonBase>(ButtonBase.CommandProperty, nameof(ButtonBase.CommandParameter), nameof(ButtonBase.Click));
        }
    }
}