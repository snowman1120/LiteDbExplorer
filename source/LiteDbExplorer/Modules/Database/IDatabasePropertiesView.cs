using Caliburn.Micro;
using LiteDB;

namespace LiteDbExplorer.Modules.Database
{
    public interface IDatabasePropertiesView : IScreen
    {
        void Init(LiteDatabase database);
    }
}