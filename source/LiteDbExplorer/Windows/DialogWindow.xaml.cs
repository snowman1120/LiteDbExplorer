using System.Windows;
using LiteDbExplorer.Controls;
using MahApps.Metro.Controls;

namespace LiteDbExplorer.Windows
{
    public partial class DialogWindow : MetroWindow
    {
        private readonly WindowController _controller;

        public DialogWindow()
        {
            InitializeComponent();
        }
        
        public DialogWindow(FrameworkElement content, WindowController controller) : this()
        {
            _controller = controller;

            controller?.BindWindow(this);

            Content = content;
        }
    }
}
