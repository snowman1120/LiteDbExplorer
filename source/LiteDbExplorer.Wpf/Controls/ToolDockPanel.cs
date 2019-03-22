using System.Windows;
using System.Windows.Controls;

namespace LiteDbExplorer.Wpf.Controls
{
    // [ContentProperty(nameof(Content))]
    public class ToolDockPanel : ItemsControl// Control
    {
        /*public static readonly DependencyProperty ContentProperty = DependencyProperty.Register(
            nameof(Content), typeof(UIElement), typeof(ToolDockPanel), new UIPropertyMetadata(null));

        public UIElement Content
        {
            get { return (UIElement) GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }*/

        static ToolDockPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToolDockPanel),
                new FrameworkPropertyMetadata(typeof(ToolDockPanel)));
        }
    }
}