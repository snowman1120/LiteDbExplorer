using System;
using System.Windows;
using System.Windows.Data;
using Caliburn.Micro;
using Dragablz;
using LiteDbExplorer.Framework.Windows;
using LiteDbExplorer.Modules.Main;

namespace LiteDbExplorer.Presentation.Behaviors
{
    public class InterTabClient : Freezable, IInterTabClient
    {
        private static readonly Lazy<InterTabClient> _current = new Lazy<InterTabClient>(() => new InterTabClient(true));

        [Obsolete("This is a design-time only constructor, use static Current instead.")]
        public InterTabClient()
        {
        }

        private InterTabClient(bool inner)
        {
        }

        public static InterTabClient Current => _current.Value;

        public string MainPartition => @"MainPartition";

        public INewTabHost<Window> GetNewHost(IInterTabClient interTabClient, object partition, TabablzControl source)
        {
            /*var tabs = new TabablzControl
            {
                InterTabController = new InterTabController
                {
                    InterTabClient = this,
                    Partition = MainPartition
                }
            };
            
            var window = new BaseDialogWindow
            {
                Title = Application.Current?.MainWindow?.Title,
                Content = tabs,
                DataContext = vm
            };

            ViewModelBinder.Bind(vm, tabs, null);
            
            return new NewTabHost<Window>(window, tabs);*/

            var vm = IoC.Get<IDocumentSet>();

            var view = new DocumentSetView();

            var older = (ITabablzControlHolder) view;

            older.TabsControl.InterTabController = new InterTabController
            {
                InterTabClient = this,
                Partition = MainPartition
            };

            var window = new BaseDialogWindow
            {
                Content = view,
                DataContext = vm
            };

            window.SetBinding(Window.TitleProperty, new Binding
            {
                Source = vm,
                Path = new PropertyPath(nameof(IDocumentSet.DisplayName))
            });
            
            ViewModelBinder.Bind(vm, view, null);
            
            return new NewTabHost<Window>(window, older.TabsControl);
        }

        public TabEmptiedResponse TabEmptiedHandler(TabablzControl tabControl, Window window)
        {
            return TabEmptiedResponse.CloseWindowOrLayoutBranch;
        }

        protected override Freezable CreateInstanceCore()
        {
            return Current;
        }
    }
}