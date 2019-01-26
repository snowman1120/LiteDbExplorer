using System.Windows;

namespace LiteDbExplorer.Controls
{
    public class StoreContext : Freezable
    {
        public StoreContext()
        {
            Current = Store.Current;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new StoreContext();
        }

        public Store Current
        {
            get => (Store)GetValue(CurrentProperty);
            protected set => SetValue(CurrentPropertyKey, value);
        }

        public static readonly DependencyPropertyKey CurrentPropertyKey =
            DependencyProperty.RegisterReadOnly(nameof(Current), typeof(Store), typeof(StoreContext), new UIPropertyMetadata(null));

        public static readonly DependencyProperty CurrentProperty = CurrentPropertyKey.DependencyProperty;
    }
}