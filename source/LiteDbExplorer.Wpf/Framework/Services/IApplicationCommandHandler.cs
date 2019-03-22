using System.Collections.Generic;
using System.Windows.Input;

namespace LiteDbExplorer.Framework.Services
{
    public interface IApplicationCommandHandler
    {
        IEnumerable<CommandBinding> CommandBindings { get; }
    }
}