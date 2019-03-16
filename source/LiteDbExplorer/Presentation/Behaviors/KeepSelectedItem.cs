using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Interactivity;

namespace LiteDbExplorer.Presentation.Behaviors
{
    public class KeepSelectedItem : Behavior<Selector>
    {
        protected override void OnAttached()
        {
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }
        
        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is Selector selector && (selector.SelectedItem == null && e.RemovedItems.Count > 0))
            {
                var itemToReselect = e.RemovedItems[0];
                if (selector.Items.Contains(itemToReselect))
                {
                    selector.SelectedItem = itemToReselect;
                }
            }
        }
    }
}