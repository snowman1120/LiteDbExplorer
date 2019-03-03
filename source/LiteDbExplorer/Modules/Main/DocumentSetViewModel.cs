using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Media;
using Caliburn.Micro;
using LiteDbExplorer.Framework;
using LiteDbExplorer.Modules.StartPage;

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
        }

        public Guid Id { get; } = Guid.NewGuid();

        public string ContentId => Id.ToString();

        public Func<IDocument> NewDocumentFactory { get; }

        public IObservableCollection<IDocument> Documents => Items;

        private ILayoutItem _activeLayoutItem;
        private ITabablzControlHolder _controlHolder;

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
            return IoC.Get<StartPageViewModel>();
        }

        protected override void OnViewLoaded(object view)
        {
            _controlHolder = view as ITabablzControlHolder;
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

        public void OpenDocument<TDoc, TNode>(TDoc model, TNode init)
            where TDoc : IDocument<TNode> where TNode : IReferenceNode
        {
            var instance = Items.OfType<IDocument<TNode>>().FirstOrDefault(p => p.InstanceId.Equals(init.InstanceId));
            if (instance != null)
            {
                ActiveItem = instance;
                return;
            }

            model.Init(init);
            OpenDocument(model);
        }

        public void OpenDocument<TDoc, TNode>(TNode init) where TDoc : IDocument<TNode> where TNode : IReferenceNode
        {
            var doc = IoC.Get<TDoc>();
            OpenDocument(doc, init);
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

            item?.Activate();

            InvalidateDisplayGroup();

            if (!ReferenceEquals(item, currentActiveItem))
            {
                RaiseActiveDocumentChanged();
            }
        }

        public override void DeactivateItem(IDocument item, bool close)
        {
            RaiseActiveDocumentChanging();

            base.DeactivateItem(item, close);

            item?.Deactivate(close);

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

        public static readonly string[] GroupDisplayColorsSet = {"#8DA3C1","#9D827B","#C1AA66","#869A87","#C97E6C","#617595","#846A62","#887E5C","#607562","#BA5E41","#3D5573","#694F47","#696658","#425E45","#8D4823"};
        public static readonly IDictionary<string, SolidColorBrush> GroupDisplayColors = new Dictionary<string, SolidColorBrush>();

        public void InvalidateDisplayGroup()
        {
            foreach (var document in Items)
            {
                document.GroupDisplayNameIsVisible = Items
                    .Where(p => p.GroupDisplayName != document.GroupDisplayName)
                    .Any(p => p.DisplayName.Equals(document.DisplayName));

                if (string.IsNullOrEmpty(document.GroupDisplayName))
                {
                    document.GroupDisplayBackground = new SolidColorBrush(Colors.Transparent);
                    continue;
                }

                if (GroupDisplayColors.ContainsKey(document.GroupDisplayName))
                {
                    document.GroupDisplayBackground = GroupDisplayColors[document.GroupDisplayName];
                }
                else
                {
                    var count = GroupDisplayColorsSet.Length;
                    var colors = GroupDisplayColors.Count;
                    var colorIndex = Math.Max(0, (count) % (colors + 1));

                    var groupDisplayColorHexa = GroupDisplayColorsSet[colorIndex];
                    var solidColorBrush = (SolidColorBrush) new BrushConverter().ConvertFrom(groupDisplayColorHexa);
                    GroupDisplayColors.Add(document.GroupDisplayName, solidColorBrush);
                    document.GroupDisplayBackground = solidColorBrush;
                }
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