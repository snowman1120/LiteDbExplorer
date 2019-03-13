using System;
using System.Windows.Controls;

namespace LiteDbExplorer.Modules.DbDocument
{
    /// <summary>
    /// Interaction logic for DocumentPreviewView.xaml
    /// </summary>
    public partial class DocumentPreviewView : UserControl
    {
        public DocumentPreviewView()
        {
            InitializeComponent();

            splitOrientationSelector.SelectionChanged += (sender, args) =>
            {
                splitContainer.Orientation = splitOrientationSelector.SelectedIndex == 0
                    ? Orientation.Vertical
                    : Orientation.Horizontal;
            };
        }
    }
}