using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dragablz;

namespace LiteDbExplorer.Modules.Main
{
    /// <summary>
    /// Interaction logic for DocumentSetView.xaml
    /// </summary>
    public partial class DocumentSetView : UserControl, ITabablzControlHolder
    {
        public DocumentSetView()
        {
            InitializeComponent();
        }

        public TabablzControl TabsControl => null; //TabablzControl;
    }
}
