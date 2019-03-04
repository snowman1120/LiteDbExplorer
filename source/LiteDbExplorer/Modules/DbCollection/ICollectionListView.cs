using LiteDbExplorer.Framework.Services;

namespace LiteDbExplorer.Modules.DbCollection
{
    public interface ICollectionListView : IListViewInteractionProvider
    {
        void UpdateView(CollectionReference collectionReference);
        void UpdateView(DocumentReference documentReference);
    }
}