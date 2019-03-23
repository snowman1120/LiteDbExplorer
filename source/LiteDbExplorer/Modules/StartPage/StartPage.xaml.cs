using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Caliburn.Micro;

namespace LiteDbExplorer.Modules.StartPage
{
    /// <summary>
    /// Interaction logic for StartPage.xaml
    /// </summary>
    public partial class StartPage : UserControl
    {
        public StartPage()
        {
            InitializeComponent();

            InvalidateLayout();

            SizeChanged += (sender, args) => { InvalidateLayout(); };
        }

        private void InvalidateLayout()
        {
            if (ActualWidth >= 1300)
            {
                contentGrid.ColumnDefinitions[1].Width = new GridLength(1200, GridUnitType.Pixel);
                contentGrid.ColumnDefinitions[0].Width = new GridLength(1, GridUnitType.Star);
                contentGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
            }
            else
            {
                contentGrid.ColumnDefinitions[1].Width = new GridLength(1, GridUnitType.Star);
                contentGrid.ColumnDefinitions[0].Width = GridLength.Auto;
                contentGrid.ColumnDefinitions[2].Width = GridLength.Auto;
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (e.Uri != null)
            {
                Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            }

            e.Handled = true;
        }
    }
}