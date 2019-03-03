using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using Caliburn.Micro;

namespace LiteDbExplorer.Framework
{
    public abstract class LayoutItemBase : Screen, ILayoutItem
    {
        private readonly Guid _id = Guid.NewGuid();
		
        public abstract ICommand CloseCommand { get; }

        [Browsable(false)]
        public Guid Id => _id;

        [Browsable(false)]
        public string ContentId => _id.ToString();

        private bool _isSelected;

        [Browsable(false)]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                _isSelected = value;
                NotifyOfPropertyChange(() => IsSelected);
            }
        }

        [Browsable(false)]
        public virtual bool ShouldReopenOnStart => false;

        public virtual object IconContent { get; set; }
    }
}