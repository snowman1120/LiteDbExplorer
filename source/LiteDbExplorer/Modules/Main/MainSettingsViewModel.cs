using System.ComponentModel.Composition;
using Caliburn.Micro;
using LiteDbExplorer.Wpf.Modules.Settings;
using PropertyTools.DataAnnotations;

namespace LiteDbExplorer.Modules.Main
{
    [Export(typeof(ISettingsEditor))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class MainSettingsViewModel : PropertyChangedBase, ISettingsEditor, IAutoGenSettingsView
    {
        public string SettingsPageName => Properties.Resources.SettingsPageGeneral;
        public string SettingsPagePath => Properties.Resources.SettingsPageEnvironment;
        public int EditorDisplayOrder => 10;

        public string GroupDisplayName => SettingsPageName.Trim('_');
        
        public object AutoGenContext => this;

        public MainSettingsViewModel()
        {
            ColorTheme = Settings.Current.ColorTheme;
        }

        [SelectorStyle(SelectorStyle.ComboBox), Width(120)]
        public ColorTheme ColorTheme { get; set; }

        public void ApplyChanges()
        {
            Settings.Current.ColorTheme = ColorTheme;
            
            Settings.Current.SaveSettings();
        }

        public void DiscardChanges()
        {
            // Ignore
        }
    }
}