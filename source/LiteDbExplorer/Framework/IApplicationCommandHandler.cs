using System.Collections.Generic;
using System.Windows.Input;

namespace LiteDbExplorer.Framework
{
    public interface IApplicationCommandHandler
    {
        IEnumerable<CommandBinding> CommandBindings { get; }
    }
}