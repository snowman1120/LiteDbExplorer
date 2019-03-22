using System.Collections.Generic;
using System.Windows;

namespace LiteDbExplorer.Framework.Windows
{
    public class DialogSettings
    {
        private readonly Dictionary<string, object> _settings;
        private double _height;
        private double _width;
        private bool _isMinButtonEnabled;
        private WindowStartupLocation _windowStartupLocation;
        private ResizeMode _resizeMode;
        private SizeToContent _sizeToContent;
        private WindowStyle _windowStyle;

        public DialogSettings()
        {
            _settings = new Dictionary<string, object>();
        }

        public IDictionary<string, object> Settings => _settings;

        public double Height
        {
            get => _height;
            set
            {
                _height = value;
                _settings[nameof(Height)] = value;
            }
        }

        public double Width
        {
            get => _width;
            set
            {
                _width = value;
                _settings[nameof(Width)] = value;
            }
        }

        public bool IsMinButtonEnabled
        {
            get => _isMinButtonEnabled;
            set
            {
                _isMinButtonEnabled = value;
                _settings[nameof(IsMinButtonEnabled)] = value;
            }
        }

        public WindowStartupLocation WindowStartupLocation
        {
            get => _windowStartupLocation;
            set
            {
                _windowStartupLocation = value;
                _settings[nameof(WindowStartupLocation)] = value;
            }
        }

        public ResizeMode ResizeMode
        {
            get => _resizeMode;
            set
            {
                _resizeMode = value;
                _settings[nameof(ResizeMode)] = value;
            }
        }

        public SizeToContent SizeToContent
        {
            get => _sizeToContent;
            set
            {
                _sizeToContent = value;
                _settings[nameof(SizeToContent)] = value;
            }
        }

        public WindowStyle WindowStyle
        {
            get => _windowStyle;
            set
            {
                _windowStyle = value;
                _settings[nameof(WindowStyle)] = value;
            }
        }
    }
}