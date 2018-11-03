using NLog;
using NLog.Targets;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;

namespace LiteDbExplorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly string _instanceMuxet = "LiteDBExplorerInstaceMutex";
        private Mutex _appMutex;

        public bool OriginalInstance
        {
            get;
            private set;
        }

        public static Settings Settings => Settings.Current;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
#if !DEBUG
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
#endif
            Config.ConfigureLogger();

            // For now we want to allow multiple instances if app is started without args
            if (Mutex.TryOpenExisting(_instanceMuxet, out var mutex))
            {
                var client = new PipeClient(Config.PipeEndpoint);

                if (e.Args.Any())
                {
                    client.InvokeCommand(CmdlineCommands.Open, e.Args[0]);                    
                    Shutdown();
                    return;
                }
            }
            else
            {
                _appMutex = new Mutex(true, _instanceMuxet);
                OriginalInstance = true;
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.SaveSettings();

            _appMutex?.ReleaseMutex();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var log = LogManager.Configuration.FindTargetByName("file") as FileTarget;
            Logger.Error((Exception)e.ExceptionObject, "Unhandled exception: ");
            MessageBox.Show(string.Format("Unhandled exception occured.\nAdditional information written into: {0}\n\nApplication will shutdown.", log.FileName),
                "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            Process.GetCurrentProcess().Kill();
        }
    }
}
