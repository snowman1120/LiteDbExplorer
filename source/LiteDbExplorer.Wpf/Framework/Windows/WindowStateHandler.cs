using System;
using System.Windows;
using LiteDbExplorer.Wpf.Framework.Windows;

namespace LiteDbExplorer
{
    public static class WindowPositionHandlerExtensions
    {
        public static void AttachPositionHandler(this Window window, IWindowStateStore store, string windowName)
        {
            var unused = new WindowStateHandler(store, window, windowName, true);
        }
    }

    public class WindowStateHandler
    {
        private readonly IWindowStateStore _store;
        private readonly Window _window;
        private readonly string _windowName;
        private bool _ignoreChanges;
        private bool _initialized;

        public WindowStateHandler(IWindowStateStore store, Window window, string windowName, bool autoAttach = false)
        {
            _store = store;
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
            RestoreSizeAndLocation(_store);

            _initialized = true;
        }

        private void WindowOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            SaveSize(_store);
        }

        private void WindowOnLocationChanged(object sender, EventArgs e)
        {
            if (!_initialized)
            {
                return;
            }

            SavePosition(_store);
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

        public void SaveSize(IWindowStateStore config)
        {
            if (config == null || _ignoreChanges)
            {
                return;
            }

            if (!config.WindowPositions.ContainsKey(_windowName))
            {
                config.WindowPositions[_windowName] = new WindowPosition();
            }

            config.WindowPositions[_windowName].Size = new WindowPosition.Point
            {
                X = _window.Width,
                Y = _window.Height
            };
        }

        public void SavePosition(IWindowStateStore config)
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
                config.WindowPositions[_windowName] = new WindowPosition();
            }

            config.WindowPositions[_windowName].Position = new WindowPosition.Point
            {
                X = _window.Left,
                Y = _window.Top
            };
        }

        public void RestoreSizeAndLocation(IWindowStateStore config)
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
