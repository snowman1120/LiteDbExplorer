using System.Windows.Controls;
using System.Windows.Interactivity;

namespace LiteDbExplorer.Presentation.Behaviors
{
    public class KeepSelected : Behavior<ListBox>
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
            if (sender is ListBox listbox && (listbox.SelectedItem == null && e.RemovedItems.Count > 0))
            {
                var itemToReselect = e.RemovedItems[0];
                if (listbox.Items.Contains(itemToReselect))
                {
                    listbox.SelectedItem = itemToReselect;
                }
            }
        }
    }
}