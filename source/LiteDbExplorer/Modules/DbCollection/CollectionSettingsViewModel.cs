using System.ComponentModel.Composition;
using System.Windows.Controls;
using Caliburn.Micro;
using LiteDbExplorer.Wpf.Modules.Settings;
using PropertyTools.DataAnnotations;
// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global

namespace LiteDbExplorer.Modules.DbCollection
{
    [Export(typeof(ISettingsEditor))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class CollectionSettingsViewModel : PropertyChangedBase, ISettingsEditor, IAutoGenSettingsView
    {
        public CollectionSettingsViewModel()
        {
            CollectionExplorer_FieldSortOrder = Settings.Current.FieldSortOrder;

            CollectionExplorer_SplitOrientation = Properties.Settings.Default.CollectionExplorer_SplitOrientation;
            CollectionExplorer_ShowPreview = Properties.Settings.Default.CollectionExplorer_ShowPreview;
            CollectionExplorer_ContentMaxLength = Properties.Settings.Default.CollectionExplorer_ContentMaxLength;
            CollectionExplorer_DoubleClickAction = Properties.Settings.Default.CollectionExplorer_DoubleClickAction;
        }

        public string SettingsPagePath => Properties.Resources.SettingsPageView;
        
        public string SettingsPageName => "_Collections";
        
        public int EditorDisplayOrder => 25;

        public string GroupDisplayName => "Options";

        public object AutoGenContext => this;

        [Category("Collection Explorer")]
        [DisplayName("Field Sort Order")]
        public FieldSortOrder CollectionExplorer_FieldSortOrder { get; set; }
        
        [Category("Collection Explorer")]
        [DisplayName("Double Click Action")]
        public CollectionItemDoubleClickAction CollectionExplorer_DoubleClickAction { get; set; }

        [Category("Collection Explorer")]
        [DisplayName("Split Orientation")]
        public Orientation CollectionExplorer_SplitOrientation { get; set; }
        
        [Category("Collection Explorer")]
        [DisplayName("Show Preview")]
        public bool CollectionExplorer_ShowPreview { get; set; }

        [Category("Collection Explorer")]
        [DisplayName("Content Max Length")]
        [Spinnable(1, 1, 64, 1024), Width(80)]
        public int CollectionExplorer_ContentMaxLength { get; set; }
        
        public void ApplyChanges()
        {
            Settings.Current.FieldSortOrder = CollectionExplorer_FieldSortOrder;
            Settings.Current.SaveSettings();

            Properties.Settings.Default.CollectionExplorer_SplitOrientation = CollectionExplorer_SplitOrientation;
            Properties.Settings.Default.CollectionExplorer_ShowPreview = CollectionExplorer_ShowPreview;
            Properties.Settings.Default.CollectionExplorer_ContentMaxLength = CollectionExplorer_ContentMaxLength;
            Properties.Settings.Default.CollectionExplorer_DoubleClickAction = CollectionExplorer_DoubleClickAction;

            Properties.Settings.Default.Save();
        }

        public void DiscardChanges()
        {
            // Ignore
        }
        
    }
}