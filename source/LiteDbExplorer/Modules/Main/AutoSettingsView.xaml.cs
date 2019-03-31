using System.Windows;
using System.Windows.Controls;
using LiteDbExplorer.Wpf.Modules.Settings;
using PropertyTools.Wpf;

namespace LiteDbExplorer.Modules.Main
{
    /// <summary>
    /// Interaction logic for AutoSettingsView.xaml
    /// </summary>
    public partial class AutoSettingsView : UserControl
    {
        public AutoSettingsView()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataContext is IAutoGenSettingsView autoGenSettingsView)
            {
                propertyGrid.Operator = new PropertyGridOperator
                {
                    DefaultCategoryName = autoGenSettingsView.GroupDisplayName
                };
            }
        }
    }
}
