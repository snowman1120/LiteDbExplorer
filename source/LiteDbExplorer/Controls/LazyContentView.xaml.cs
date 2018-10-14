using System.Windows.Controls;

namespace LiteDbExplorer.Controls
{
    /// <summary>
    /// Interaction logic for LazyContentView.xaml
    /// </summary>
    public partial class LazyContentView : UserControl
    {
        public LazyContentView()
        {
            InitializeComponent();
        }

        public bool ContentLoaded => ContentPresenter.Content != null;

        public new object Content
        {
            get => ContentPresenter.Content;
            set => ContentPresenter.Content = value;
        }
    }
}
