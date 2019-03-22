namespace LiteDbExplorer.Framework.Services
{
    public interface IListViewInteractionProvider
    {
        void ScrollIntoItem(object item);

        void ScrollIntoSelectedItem();
    }
}