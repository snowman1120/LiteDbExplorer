using NLog;
using NLog.Targets;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using LiteDbExplorer.Controls;
using LiteDbExplorer.Framework.Windows;
using LiteDbExplorer.Presentation;

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
        private bool _errorNotified;

        public bool OriginalInstance { get; private set; }
        
        public static Settings Settings => Settings.Current;

        public App()
        {
            AppDomain.CurrentDomain.UnhandledException += OnCurrentDomainUnhandledException;
            DispatcherUnhandledException += OnDispatcherUnhandledException;

            Config.ConfigureLogger();
        }

        protected override void OnStartup(StartupEventArgs e)
        {

            if (Resources["bootstrapper"] == null)
            {
                StartupUri = new System.Uri(@"Windows\MainWindow.xaml", System.UriKind.Relative);
            }
            else
            {
                ShutdownMode = ShutdownMode.OnLastWindowClose;
            }

            base.OnStartup(e);
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            ThemeManager.InitColorTheme(Settings.ColorTheme);


            Settings.PropertyChanged -= Settings_PropertyChanged;
            Settings.PropertyChanged += Settings_PropertyChanged;
            
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
        
        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(Settings.ColorTheme):
                    SetColorTheme();
                    break;
            }
        }

        private void SetColorTheme()
        {
            ThemeManager.SetColorTheme(Settings.ColorTheme);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            Settings.SaveSettings();

            _appMutex?.ReleaseMutex();
        }

        private void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var log = LogManager.Configuration.FindTargetByName("file") as FileTarget;
            Logger.Error((Exception) e.ExceptionObject, "Unhandled exception: ");

            var message = "Unhandled exception occured.\n";
            if (log != null)
            {
                message += $"\nAdditional information written into: {log.FileName}.\n";
            }
            if (e.IsTerminating)
            {
                message += "\nApplication will shutdown.\n";
            }
            
            ShowError((Exception) e.ExceptionObject, message, "Unhandled Exception");
            
            if (e.IsTerminating)
            {
                Process.GetCurrentProcess().Kill();
            }
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            var log = LogManager.Configuration.FindTargetByName("file") as FileTarget;
            Logger.Error(e.Exception, "An unhandled exception occurred");

            var message = "Unhandled exception occured.\n";
            if (log != null)
            {
                message += $"\nAdditional information written into: {log.FileName}.\n";
            }
            if (!e.Handled)
            {
                message += "\nApplication will shutdown.\n";
            }

            ShowError(e.Exception, message, "Unhandled Exception");
        }

        public void ShowError(Exception exception, string message, string caption = "")
        {
            if(_errorNotified)
            {
                return;
            }
            
            _errorNotified = true;

            var exceptionViewer = new ExceptionViewer(message, exception);
            var baseDialogWindow = new BaseDialogWindow
            {
                Title = string.IsNullOrEmpty(caption) ? "Error" : caption,
                Content = exceptionViewer,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                ResizeMode = ResizeMode.CanResizeWithGrip,
                MinHeight = 400,
                MinWidth = 500,
                ShowMinButton = false,
                ShowMaxRestoreButton = false,
                ShowInTaskbar = false
            };
            baseDialogWindow.ShowDialog();

            // MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}