using System.Windows;

namespace LiteDbExplorer
{
    public class WindowPositionHandler
    {
        private readonly Window _window;
        private readonly string _windowName;
        private bool _ignoreChanges;

        public WindowPositionHandler(Window window, string windowName)
        {
            _window = window;
            _windowName = windowName;
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
