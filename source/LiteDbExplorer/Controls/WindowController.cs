using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using JetBrains.Annotations;

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
        
        public virtual Window InferOwnerOf(Window window)
        {
            var current = Application.Current;
            if (current == null)
            {
                return null;
            }

            var ownerWindow = current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive) ?? (PresentationSource.FromVisual(current.MainWindow) == null ? null : current.MainWindow);
            if (ownerWindow != window)
            {
                return ownerWindow;
            }

            return null;
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
#pragma warning disable CS0067 // The event 'WindowController.PropertyChanged' is never used
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067 // The event 'WindowController.PropertyChanged' is never used
    }
}