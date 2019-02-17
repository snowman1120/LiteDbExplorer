using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Windows.Input;

namespace LiteDbExplorer.Framework
{
    [Export(typeof(IApplicationCommandHandler))]
    [PartCreationPolicy (CreationPolicy.Shared)]
    [InheritedExport(typeof(IApplicationCommandHandler))]
    public abstract class ApplicationCommandHandler : IApplicationCommandHandler
    {
        private readonly IList<CommandBinding> _commands;

        protected ApplicationCommandHandler()
        {
            _commands = new List<CommandBinding>();
        }

        public void Add(CommandBinding commandBinding)
        {
            _commands.Add(commandBinding);
        }

        public void Add(ICommand command, ExecutedRoutedEventHandler executed)
        {
            _commands.Add(new CommandBinding(command, executed));
        }

        public void Add(ICommand command, ExecutedRoutedEventHandler executed, CanExecuteRoutedEventHandler canExecute)
        {
            _commands.Add(new CommandBinding(command, executed, canExecute));
        }

        public IEnumerable<CommandBinding> CommandBindings => _commands;
    }
}