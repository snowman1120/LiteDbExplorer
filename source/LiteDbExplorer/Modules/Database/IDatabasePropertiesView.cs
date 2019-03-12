using Caliburn.Micro;

namespace LiteDbExplorer.Modules.Database
{
    public interface IDatabasePropertiesView : IScreen
    {
        void Init(DatabaseReference database);
    }
}