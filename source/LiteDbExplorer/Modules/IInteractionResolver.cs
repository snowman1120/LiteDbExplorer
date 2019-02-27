using LiteDB;

namespace LiteDbExplorer.Modules
{
    public interface IInteractionResolver
    {
        bool ShowDatabaseProperties(LiteDatabase database);
        bool OpenEditDocument(DocumentReference document);
    }
}