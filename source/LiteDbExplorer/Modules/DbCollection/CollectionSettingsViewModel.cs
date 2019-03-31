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
        }

        public string SettingsPagePath => Properties.Resources.SettingsPageView;
        
        public string SettingsPageName => "_Collections";
        
        public int EditorDisplayOrder => 20;

        public string GroupDisplayName => "Options";

        public object AutoGenContext => this;

        [Category("Collection Explorer")]
        [DisplayName("Field Sort Order")]
        public FieldSortOrder CollectionExplorer_FieldSortOrder { get; set; }
        
        [Category("Collection Explorer")]
        [DisplayName("Double Click Action")]
        public DocumentItemClickAction CollectionExplorer_DoubleClickAction { get; set; }

        [Category("Collection Explorer")]
        [DisplayName("Split Orientation")]
        public Orientation CollectionExplorer_SplitOrientation { get; set; }
        
        [Category("Collection Explorer")]
        [DisplayName("Show Preview")]
        public bool CollectionExplorer_ShowPreview { get; set; }

        [Category("Collection Explorer")]
        [DisplayName("Content Max Length")]
        [Spinnable(1, 1, 120, 1024), Width(80)]
        public int CollectionExplorer_ContentMaxLength { get; set; } = 1024;
        
        [Category("Document Preview")]
        [DisplayName("Split Orientation")]
        public Orientation DocumentPreview_SplitOrientation { get; set; }

        [Category("Document Preview")]
        [DisplayName("Content Max Length")]
        [Spinnable(1, 1, 120, 1024), Width(80)]
        public int DocumentPreview_ContentMaxLength { get; set; } = 1024;

        public void ApplyChanges()
        {
            Settings.Current.FieldSortOrder = CollectionExplorer_FieldSortOrder;
            Settings.Current.SaveSettings();
        }

        public void DiscardChanges()
        {
            // Ignore
        }
        
    }

    public enum DocumentItemClickAction
    {
        [System.ComponentModel.Description("Edit document")]
        EditDocument,

        [System.ComponentModel.Description("Open preview in tab")]
        OpenPreview
    }
}