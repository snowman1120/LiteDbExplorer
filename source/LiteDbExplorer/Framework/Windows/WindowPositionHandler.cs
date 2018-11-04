using System;
using System.Windows;

namespace LiteDbExplorer
{
    public static class WindowPositionHandlerExtensions
    {
        public static void AttachPositionHandler(this Window window, string windowName)
        {
            var handler = new WindowPositionHandler(window, windowName, true);
        }
    }

    public class WindowPositionHandler
    {
        private readonly Window _window;
        private readonly string _windowName;
        private bool _ignoreChanges;
        private bool _initialized;

        public WindowPositionHandler(Window window, string windowName, bool autoAttach = false)
        {
            _window = window;
            _windowName = windowName;

            if (autoAttach)
            {
                _window.Loaded += WindowOnLoaded;
                _window.LocationChanged += WindowOnLocationChanged;
                _window.SizeChanged += WindowOnSizeChanged;
                _window.Unloaded += WindowOnUnloaded;
            }
        }
        
        private void WindowOnLoaded(object sender, RoutedEventArgs e)
        {
            RestoreSizeAndLocation(App.Settings);

            _initialized = true;
        }

        private void WindowOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            SaveSize(App.Settings);   
        }

        private void WindowOnLocationChanged(object sender, EventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            SavePosition(App.Settings);
        }

        private void WindowOnUnloaded(object sender, RoutedEventArgs e)
        {
            if (_window != null)
            {
                _window.Loaded -= WindowOnLoaded;
                _window.LocationChanged -= WindowOnLocationChanged;
                _window.SizeChanged -= WindowOnSizeChanged;
                _window.Unloaded -= WindowOnUnloaded;
            }
        }

        public void SaveSize(Settings config)
        {
            if (config == null || _ignoreChanges)
            {
                return;
            }

            if (!config.WindowPositions.ContainsKey(_windowName))
            {
                config.WindowPositions[_windowName] = new Settings.WindowPosition();
            }

            config.WindowPositions[_windowName].Size = new Settings.WindowPosition.Point()
            {
                X = _window.Width,
                Y = _window.Height
            };
        }

        public void SavePosition(Settings config)
        {
            if (config == null || _ignoreChanges)
            {
                return;
            }

            if (_window.Left < 0 || _window.Top < 0)
            {
                return;
            }

            if (!config.WindowPositions.ContainsKey(_windowName))
            {
                config.WindowPositions[_windowName] = new Settings.WindowPosition();
            }

            config.WindowPositions[_windowName].Position = new Settings.WindowPosition.Point()
            {
                X = _window.Left,
                Y = _window.Top
            };
        }

        public void RestoreSizeAndLocation(Settings config)
        {
            if (!config.WindowPositions.ContainsKey(_windowName))
            {
                return;
            }

            _ignoreChanges = true;

            try
            {
                var data = config.WindowPositions[_windowName];

                if (data.Position != null)
                {
                    _window.Left = data.Position.X;
                    _window.Top = data.Position.Y;
                }

                if (data.Size != null)
                {
                    _window.Width = data.Size.X;
                    _window.Height = data.Size.Y;
                }
            }
            finally
            {
                _ignoreChanges = false;
            }
        }
    }
}
