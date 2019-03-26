using System;
using System.ComponentModel.Composition;
using System.Windows;
using NLog;

namespace LiteDbExplorer.Modules
{
    [Export(typeof(PipeServiceBootstrapper))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class PipeServiceBootstrapper
    {
        private readonly IDatabaseInteractions _databaseInteractions;
        private static readonly NLog.Logger Logger = LogManager.GetCurrentClassLogger();

        private PipeService _pipeService;
        private PipeServer _pipeServer;

        [ImportingConstructor]
        public PipeServiceBootstrapper(IDatabaseInteractions databaseInteractions)
        {
            _databaseInteractions = databaseInteractions;
        }

        public void Init()
        {
            if ((Application.Current as App)?.OriginalInstance == true)
            {
                _pipeService = new PipeService();
                _pipeService.CommandExecuted += PipeService_CommandExecuted;
                _pipeServer = new PipeServer(Config.PipeEndpoint);
                _pipeServer.StartServer(_pipeService);

                var args = Environment.GetCommandLineArgs();
                if (args.Length > 1)
                {
                    PipeService_CommandExecuted(this, new CommandExecutedEventArgs(CmdlineCommands.Open, args[1]));
                }
            }
        }

        private void PipeService_CommandExecuted(object sender, CommandExecutedEventArgs args)
        {
            Logger.Info(@"Executing command ""{0}"" from pipe with arguments ""{1}""", args.Command, args.Args);

            switch (args.Command)
            {
                case CmdlineCommands.Focus:
                    RestoreWindow();
                    break;

                case CmdlineCommands.Open:
                    _databaseInteractions.OpenDatabase(args.Args);
                    RestoreWindow();
                    break;

                default:
                    Logger.Warn("Unknown command received");
                    break;
            }
        }

        private void RestoreWindow()
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
            {
                // Show();
                // WindowState = WindowState.Normal;
                mainWindow.Activate();
                mainWindow.Focus();
            }
        }
    }
}