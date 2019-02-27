using System.ComponentModel.Composition;
using LiteDbExplorer.Framework;

namespace LiteDbExplorer.Modules.Collection
{
    [Export(typeof(CollectionExplorerViewModel))]
    [PartCreationPolicy (CreationPolicy.NonShared)]
    public class CollectionExplorerViewModel : Document
    {
    }
}
