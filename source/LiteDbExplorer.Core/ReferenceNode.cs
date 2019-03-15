using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using LiteDbExplorer.Core;

namespace LiteDbExplorer
{
    public class ReferenceChangedEventArgs<T> : EventArgs
    {
        public ReferenceChangedEventArgs(ReferenceNodeChangeAction action, T reference)
        {
            Action = action;
            Reference = reference;
        }

        public ReferenceNodeChangeAction Action { get; }
        public T Reference { get; }
    }

    public class CollectionReferenceChangedEventArgs<T> : EventArgs
    {
        public CollectionReferenceChangedEventArgs(ReferenceNodeChangeAction action, IEnumerable<T> items)
        {
            Action = action;
            Items = items;
        }

        public ReferenceNodeChangeAction Action { get; }
        public IEnumerable<T> Items { get; }
    }

    public abstract class ReferenceNode<T> : INotifyPropertyChanging, INotifyPropertyChanged, IReferenceNode
    {
        protected ReferenceNode()
        {
            InstanceId = Guid.NewGuid().ToString();
        }

        public virtual string InstanceId { get; }

        public virtual bool ReferenceEquals(ReferenceNode<T> reference)
        {
            return InstanceId.Equals(reference?.InstanceId);
        }

        public event EventHandler<ReferenceChangedEventArgs<T>> ReferenceChanged;
        
        internal virtual void OnReferenceChanged(ReferenceNodeChangeAction action, T item)
        {
            OnReferenceChanged(new ReferenceChangedEventArgs<T>(action, item));
        }

        protected virtual void OnReferenceChanged(ReferenceChangedEventArgs<T> e)
        {
            ReferenceChanged?.Invoke(this, e);
        }

        [UsedImplicitly]
        public event PropertyChangedEventHandler PropertyChanged;
        
        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangingEventHandler PropertyChanging;
        
        protected virtual void OnPropertyChanging([CallerMemberName] string name = null)
        {
            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(name));
        }
    }
}