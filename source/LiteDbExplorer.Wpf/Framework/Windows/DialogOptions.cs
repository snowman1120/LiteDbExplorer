using System.Collections.Generic;
using System.Windows;

namespace LiteDbExplorer.Framework.Windows
{
    public class DialogOptions
    {
        private readonly Dictionary<string, object> _value;
        private double _height;
        private double _width;
        private bool _isMinButtonEnabled;
        private WindowStartupLocation _windowStartupLocation;
        private ResizeMode _resizeMode;
        private SizeToContent _sizeToContent;
        private WindowStyle _windowStyle;

        public DialogOptions()
        {
            _value = new Dictionary<string, object>();
        }

        public IDictionary<string, object> Value => _value;

        public double Height
        {
            get => _height;
            set
            {
                _height = value;
                _value[nameof(Height)] = value;
            }
        }

        public double Width
        {
            get => _width;
            set
            {
                _width = value;
                _value[nameof(Width)] = value;
            }
        }

        public bool IsMinButtonEnabled
        {
            get => _isMinButtonEnabled;
            set
            {
                _isMinButtonEnabled = value;
                _value[nameof(IsMinButtonEnabled)] = value;
            }
        }

        public WindowStartupLocation WindowStartupLocation
        {
            get => _windowStartupLocation;
            set
            {
                _windowStartupLocation = value;
                _value[nameof(WindowStartupLocation)] = value;
            }
        }

        public ResizeMode ResizeMode
        {
            get => _resizeMode;
            set
            {
                _resizeMode = value;
                _value[nameof(ResizeMode)] = value;
            }
        }

        public SizeToContent SizeToContent
        {
            get => _sizeToContent;
            set
            {
                _sizeToContent = value;
                _value[nameof(SizeToContent)] = value;
            }
        }

        public WindowStyle WindowStyle
        {
            get => _windowStyle;
            set
            {
                _windowStyle = value;
                _value[nameof(WindowStyle)] = value;
            }
        }
    }
}