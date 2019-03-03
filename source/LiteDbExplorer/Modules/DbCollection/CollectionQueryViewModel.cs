using System.ComponentModel.Composition;

namespace LiteDbExplorer.Modules.DbCollection
{
    [Export(typeof(CollectionQueryViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class CollectionQueryViewModel : Framework.Document
    {
    }
}
