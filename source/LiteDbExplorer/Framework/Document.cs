using System.Windows.Input;
using System.Windows.Media;

namespace LiteDbExplorer.Framework
{
    public abstract class Document : LayoutItemBase, IDocument
    {
        public string GroupDisplayName { get; set; }

        public bool GroupDisplayNameIsVisible { get; set; }

        public SolidColorBrush GroupDisplayBackground { get; set; }

        private ICommand _closeCommand;
        public override ICommand CloseCommand
        {
            get { return _closeCommand ?? (_closeCommand = new RelayCommand(p => TryClose(null), p => true)); }
        }
    }

    public abstract class Document<T> : Document, IDocument<T> where T : IReferenceNode
    {
        public abstract string InstanceId { get; }

        public abstract void Init(T item);
    }
}