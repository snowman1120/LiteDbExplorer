using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;
using Dragablz;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Presentation;
using LiteDbExplorer.Wpf.Framework;

namespace LiteDbExplorer.Modules.Main
{
    [Export(typeof(IDocumentSet))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class DocumentSetViewModel : Conductor<IDocument>.Collection.OneActive, IDocumentSet
    {
#pragma warning disable 649
        private bool _closing;
#pragma warning restore 649
        
        public DocumentSetViewModel()
        {
            DisplayName = $"LiteDB Explorer {Versions.CurrentVersion}";

            NewDocumentFactory = NewDocumentFactoryHandler;

            CloseDocumentCommand = new RelayCommand<FrameworkElement>(CloseDocument);
        }
        
        public Guid Id { get; } = Guid.NewGuid();

        public string ContentId => Id.ToString();

        public Func<IDocument> NewDocumentFactory { get; }

        public ICommand CloseDocumentCommand { get; }
        
        public IObservableCollection<IDocument> Documents => Items;

        private ILayoutItem _activeLayoutItem;
        
        public ILayoutItem ActiveLayoutItem
        {
            get => _activeLayoutItem;
            set
            {
                if (ReferenceEquals(_activeLayoutItem, value))
                {
                    return;
                }

                _activeLayoutItem = value;

                if (value is IDocument document)
                {
                    ActivateItem(document);
                }

                NotifyOfPropertyChange(() => ActiveLayoutItem);
            }
        }

        private IDocument NewDocumentFactoryHandler()
        {
            return IoC.Get<IStartupDocument>();
        }
        
        public void OpenDocument(IDocument model)
        {
            ActivateItem(model);
            model.IsSelected = true;
        }

        public void OpenDocument<TDoc>() where TDoc : IDocument
        {
            var doc = IoC.Get<TDoc>();
            OpenDocument(doc);
        }

        public TDoc OpenDocument<TDoc, TNode>(TDoc model, TNode init)
            where TDoc : IDocument<TNode> where TNode : IReferenceNode
        {
            if (!string.IsNullOrEmpty(init.InstanceId))
            {
                var instance = Items.OfType<TDoc>().FirstOrDefault(p => !string.IsNullOrEmpty(p.InstanceId) && p.InstanceId.Equals(init.InstanceId));
                if (instance != null)
                {
                    ActiveItem = instance;
                    return instance;
                }
            }
            
            model.Init(init);

            OpenDocument(model);

            return model;
        }

        public TDoc OpenDocument<TDoc, TNode>(TNode init) where TDoc : IDocument<TNode> where TNode : IReferenceNode
        {
            var model = IoC.Get<TDoc>();
            OpenDocument(model, init);
            return model;
        }

        public void CloseDocument(FrameworkElement element)
        {
            if (element.DataContext is IDocument document)
            {
                CloseDocument(document);
            }
        }

        public void CloseDocument(IDocument document)
        {
            DeactivateItem(document, true);
        }

        public override void ActivateItem(IDocument item)
        {
            if (_closing)
            {
                return;
            }

            RaiseActiveDocumentChanging();

            var currentActiveItem = ActiveItem;
            
            base.ActivateItem(item);
            
            if (item != null)
            {
                item.Activate();
                item.IsSelected = true;
            }
            
            InvalidateDisplayGroup();

            if (!ReferenceEquals(item, currentActiveItem))
            {
                RaiseActiveDocumentChanged();
            }
        }

        public override void DeactivateItem(IDocument item, bool close)
        {
            RaiseActiveDocumentChanging();
            
            if (close == false)
            {
                item?.Deactivate(false);
            }

            base.DeactivateItem(item, close);
            
            InvalidateDisplayGroup();
            
            RaiseActiveDocumentChanged();
        }
        
        protected override void OnActivationProcessed(IDocument item, bool success)
        {
            if (!ReferenceEquals(ActiveLayoutItem, item))
            {
                ActiveLayoutItem = item;
            }

            base.OnActivationProcessed(item, success);
        }
        
        public void InvalidateDisplayGroup()
        {
            foreach (var document in Items)
            {
                document.GroupDisplayNameIsVisible = Items
                    .Where(p => p.GroupDisplayName != document.GroupDisplayName)
                    .Any(p => p.DisplayName.Equals(document.DisplayName));

                document.GroupDisplayBackground =
                    GroupDisplayColor.GetDisplayColor(document.GroupId, Colors.Transparent);
            }
        }

        private void RaiseActiveDocumentChanging()
        {
            var handler = ActiveDocumentChanging;
            handler?.Invoke(this, EventArgs.Empty);
        }

        private void RaiseActiveDocumentChanged()
        {
            var handler = ActiveDocumentChanged;
            handler?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ActiveDocumentChanging;
        public event EventHandler ActiveDocumentChanged;
    }
}