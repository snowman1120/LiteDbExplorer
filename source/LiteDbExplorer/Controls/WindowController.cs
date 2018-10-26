using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using LiteDbExplorer.Annotations;

namespace LiteDbExplorer.Controls
{
    public class WindowController : INotifyPropertyChanged
    {
        private Window _window;
        
        public Action Activated { get; set; }

        public string Title { get; set; } = string.Empty;

        public void Close(bool dialogResult)
        {
            if (_window != null)
            {
                _window.DialogResult = dialogResult;
                _window.Close();
            }
        }

        public void BindWindow(Window window)
        {
            UnbindWindow();

            _window = window;
            _window.Activated += WindowOnActivated;
            _window.Closed += WindowOnClosed;

            _window.SetBinding(Window.TitleProperty, new Binding
            {
                Source = this,
                Path = new PropertyPath(nameof(Title)),
                Mode = BindingMode.TwoWay,
                FallbackValue = string.Empty
            });
        }
        
        private void UnbindWindow()
        {
            if (_window != null)
            {
                _window.Activated -= WindowOnActivated;
                _window.Closed -= WindowOnClosed;
            }

            // _window = null;
        }
        
        private void WindowOnActivated(object sender, EventArgs e)
        {
            Activated?.Invoke();
        }

        private void WindowOnClosed(object sender, EventArgs e)
        {
            UnbindWindow();
        }

        [UsedImplicitly]
        public event PropertyChangedEventHandler PropertyChanged;
    }
}