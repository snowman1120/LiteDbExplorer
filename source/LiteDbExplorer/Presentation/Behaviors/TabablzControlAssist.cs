using System.Windows;
using System.Windows.Input;

namespace LiteDbExplorer.Presentation.Behaviors
{
    public class TabablzControlAssist
    {
        // Replaced in template PART_CloseButton Command="{x:Static dragablz:TabablzControl.CloseItemCommand}"

        public static readonly DependencyProperty CloseItemCommandProperty = DependencyProperty.RegisterAttached(
            "CloseItemCommand",
            typeof(ICommand),
            typeof(TabablzControlAssist),
            new FrameworkPropertyMetadata(OnCloseItemCommandChanged)
        );
        
        public static void SetCloseItemCommand(UIElement element, ICommand value)
        {
            element.SetValue(CloseItemCommandProperty, value);
        }

        public static ICommand GetCloseItemCommand(UIElement element)
        {
            return (ICommand)element.GetValue(CloseItemCommandProperty);
        }

        private static void OnCloseItemCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var newValue = e.NewValue;
        }
    }
}