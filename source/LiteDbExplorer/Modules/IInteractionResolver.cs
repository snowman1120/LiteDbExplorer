using LiteDB;

namespace LiteDbExplorer.Modules
{
    public interface IInteractionResolver
    {
        void ShowDatabaseProperties(LiteDatabase database);
    }
}