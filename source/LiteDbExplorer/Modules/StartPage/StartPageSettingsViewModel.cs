using System.ComponentModel.Composition;
using Caliburn.Micro;
using LiteDbExplorer.Wpf.Modules.Settings;

namespace LiteDbExplorer.Modules.StartPage
{
    [Export(typeof(ISettingsEditor))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class StartPageSettingsViewModel : PropertyChangedBase, ISettingsEditor, IAutoGenSettingsView
    {
        public StartPageSettingsViewModel()
        {
            ShowStartPageOnOpen = Properties.Settings.Default.ShowStartPageOnOpen;
            ShowStartOnCloseAll = Properties.Settings.Default.ShowStartOnCloseAll;
        }

        public string SettingsPageName => Properties.Resources.SettingsPageGeneral;
        
        public string SettingsPagePath => Properties.Resources.SettingsPageEnvironment;

        public int EditorDisplayOrder => 20;

        public string GroupDisplayName => "Startup page";
        
        public object AutoGenContext => this;

        public bool ShowStartPageOnOpen { get; set; }

        public bool ShowStartOnCloseAll { get; set; }

        public void ApplyChanges()
        {
            Properties.Settings.Default.ShowStartPageOnOpen = ShowStartPageOnOpen;
            Properties.Settings.Default.ShowStartOnCloseAll = ShowStartOnCloseAll;
            Properties.Settings.Default.Save();
        }

        public void DiscardChanges()
        {
            // Ignore
        }
    }
}