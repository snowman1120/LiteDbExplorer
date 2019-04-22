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
        private bool _showMinButton = true;
        private bool _showMaxRestoreButton = true;
        private bool _showDialogsOverTitleBar;
        private bool _showIconOnTitleBar = true;
        private bool _showInTaskbar = true;

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

        public bool ShowMinButton
        {
            get => _showMinButton;
            set
            {
                _showMinButton = value;
                _value[nameof(ShowMinButton)] = value;
            }
        }

        public bool ShowMaxRestoreButton
        {
            get => _showMaxRestoreButton;
            set
            {
                _showMaxRestoreButton = value;
                _value[nameof(ShowMaxRestoreButton)] = value;
            }
        }

        public bool ShowDialogsOverTitleBar
        {
            get => _showDialogsOverTitleBar;
            set
            {
                _showDialogsOverTitleBar = value;
                _value[nameof(ShowDialogsOverTitleBar)] = value;
            }
        }
        
        public bool ShowIconOnTitleBar
        {
            get => _showIconOnTitleBar;
            set
            {
                _showIconOnTitleBar = value;
                _value[nameof(ShowIconOnTitleBar)] = value;
            }
        }

        public bool ShowInTaskbar
        {
            get => _showInTaskbar;
            set
            {
                _showInTaskbar = value;
                _value[nameof(ShowInTaskbar)] = value;
            }
        }
    }
}