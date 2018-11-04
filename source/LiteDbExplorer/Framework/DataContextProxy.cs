using System.Windows;

namespace LiteDbExplorer.Framework
{
    public class DataContextProxy : Freezable
    {
        #region Overrides of Freezable

        protected override Freezable CreateInstanceCore()
        {
            return new DataContextProxy();
        }

        #endregion

        public object Data
        {
            get => (object)GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(DataContextProxy), new UIPropertyMetadata(null));
    }
}