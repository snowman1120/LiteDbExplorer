using System;
using System.Windows;
using System.Windows.Controls;

namespace LiteDbExplorer.Controls
{
    /// <summary>
    /// Interaction logic for LazyContentView.xaml
    /// </summary>
    public partial class ContentExpander : UserControl
    {
        public ContentExpander()
        {
            InitializeComponent();

            ButtonClose.IsEnabled = false;

            ContentPresenter.DataContextChanged += ContentPresenterOnDataContextChanged;

            ButtonClose.Click += ButtonCloseOnClick;
        }
        
        public bool ContentLoaded => ContentPresenter.Content != null;

        public new object Content
        {
            get => ContentPresenter.Content;
            set => ContentPresenter.Content = value;
        }


        private void ContentPresenterOnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            InvalidateCloseButton();
        }

        private void ButtonCloseOnClick(object sender, RoutedEventArgs e)
        {
            if (ContentPresenter.Content is IDisposable disposable)
            {
                disposable.Dispose();
            }

            ContentPresenter.Content = null;

            InvalidateCloseButton();            
        }

        public void InvalidateCloseButton()
        {
            ButtonClose.IsEnabled = ContentPresenter.Content != null;
        }

    }
}
