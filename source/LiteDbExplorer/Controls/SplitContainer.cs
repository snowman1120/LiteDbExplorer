using System.Windows;
using System.Windows.Controls;

namespace LiteDbExplorer.Controls
{
    public class SplitContainer : Control
    {
        public Orientation Orientation
        {
            get => (Orientation) GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }
        
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(SplitContainer),
                new PropertyMetadata(Orientation.Horizontal));

        public UIElement FirstChild
        {
            get => (UIElement) GetValue(FirstChildProperty);
            set => SetValue(FirstChildProperty, value);
        }
        
        public static readonly DependencyProperty FirstChildProperty =
            DependencyProperty.Register(nameof(FirstChild), typeof(UIElement), typeof(SplitContainer),
                new PropertyMetadata(null));

        public UIElement SecondChild
        {
            get => (UIElement) GetValue(SecondChildProperty);
            set => SetValue(SecondChildProperty, value);
        }
        
        public static readonly DependencyProperty SecondChildProperty =
            DependencyProperty.Register(nameof(SecondChild), typeof(UIElement), typeof(SplitContainer),
                new PropertyMetadata(null));


        /*public static readonly DependencyProperty FirstChildIsCollapsedProperty = DependencyProperty.Register(
            nameof(FirstChildIsCollapsed), typeof(bool), typeof(SplitContainer), new PropertyMetadata(false));

        public bool FirstChildIsCollapsed
        {
            get => (bool) GetValue(FirstChildIsCollapsedProperty);
            set => SetValue(FirstChildIsCollapsedProperty, value);
        }*/

        public static readonly DependencyProperty SecondChildIsCollapsedProperty = DependencyProperty.Register(
            nameof(SecondChildIsCollapsed), typeof(bool), typeof(SplitContainer), new PropertyMetadata(false));

        public bool SecondChildIsCollapsed
        {
            get => (bool) GetValue(SecondChildIsCollapsedProperty);
            set => SetValue(SecondChildIsCollapsedProperty, value);
        }

        static SplitContainer()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitContainer),
                new FrameworkPropertyMetadata(typeof(SplitContainer)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }
    }
}