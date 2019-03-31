using System.ComponentModel.Composition.Hosting;
using System.Reflection;

namespace LiteDbExplorer.Wpf
{
    public class LiteDbExplorerWpfCatalog
    {
        public static Assembly Assembly = typeof(LiteDbExplorerWpfCatalog).Assembly;

        public static AssemblyCatalog AssemblyCatalog = new AssemblyCatalog(Assembly);
    }
}