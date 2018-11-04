using System;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Modules.StartPage;

namespace LiteDbExplorer.Modules.Main
{
    public interface IDocumentSet : IScreen, IHaveActiveItem
    {
        Guid Id { get; }
        string ContentId { get; }
        Func<IDocument> NewDocumentFactory { get; }
        IObservableCollection<IDocument> Documents { get; }
    }

    [Export(typeof(IDocumentSet))]
    [PartCreationPolicy (CreationPolicy.NonShared)]
    public class DocumentSetViewModel : Conductor<IDocument>.Collection.OneActive, IDocumentSet
    {
        private bool _closing;

        public DocumentSetViewModel()
        {
            DisplayName = $"LiteDB Explorer {Versions.CurrentVersion}";

            NewDocumentFactory = NewDocumentFactoryHandler;
        }

        public Guid Id { get; } = Guid.NewGuid();

        public string ContentId => Id.ToString();

        public Func<IDocument> NewDocumentFactory { get; }

        public IObservableCollection<IDocument> Documents => Items;

        private ILayoutItem _activeLayoutItem;
        public ILayoutItem ActiveLayoutItem
        {
            get => _activeLayoutItem;
            set
            {
                if (ReferenceEquals(_activeLayoutItem, value))
                    return;

                _activeLayoutItem = value;

                if (value is IDocument document)
                    ActivateItem(document);

                NotifyOfPropertyChange(() => ActiveLayoutItem);
            }
        }

        private IDocument NewDocumentFactoryHandler()
        {
            return new StartPageViewModel();
        }

        public void OpenDocument(IDocument model)
        {
            ActivateItem(model);
        }

        public void CloseDocument(IDocument document)
        {
            DeactivateItem(document, true);
        }
        
        public override void ActivateItem(IDocument item)
        {
            if (_closing)
                return;

            RaiseActiveDocumentChanging();

            var currentActiveItem = ActiveItem;

            base.ActivateItem(item);

            if (!ReferenceEquals(item, currentActiveItem))
                RaiseActiveDocumentChanged();
        }

        public override void DeactivateItem(IDocument item, bool close)
        {
            RaiseActiveDocumentChanging();

            base.DeactivateItem(item, close);

            RaiseActiveDocumentChanged();
        }

        protected override void OnActivationProcessed(IDocument item, bool success)
        {
            if (!ReferenceEquals(ActiveLayoutItem, item))
                ActiveLayoutItem = item;

            base.OnActivationProcessed(item, success);
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