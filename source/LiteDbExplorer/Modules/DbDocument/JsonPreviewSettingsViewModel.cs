using System.ComponentModel.Composition;
using Caliburn.Micro;
using LiteDbExplorer.Controls.Editor;
using LiteDbExplorer.Wpf.Modules.Settings;

namespace LiteDbExplorer.Modules.DbDocument
{
    /*[Export(typeof(ISettingsEditor))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class JsonPreviewSettingsViewModel : PropertyChangedBase, ISettingsEditor, IAutoGenSettingsView
    {
        public JsonPreviewSettingsViewModel()
        {
            // JsonPreviewOptions = new ExtendedTextEditorOptions(Settings.Default.TextEditor_JsonPreviewOptions);
            JsonPreviewOptions = new TextEditorOptionsMetadata();
        }
        
        public string SettingsPagePath => Properties.Resources.SettingsPageView;

        public string SettingsPageName => "Json Preview";

        public int EditorDisplayOrder => 50;

        public string GroupDisplayName => SettingsPageName;

        public object AutoGenContext => JsonPreviewOptions;
        
        public void ResetDefault()
        {
            JsonPreviewOptions = new TextEditorOptionsMetadata();
            NotifyOfPropertyChange(() => JsonPreviewOptions);
        }

        public TextEditorOptionsMetadata JsonPreviewOptions { get; private set; }

        public void ApplyChanges()
        {
            // TODO

            // Settings.Default.TextEditor_JsonPreviewOptions.CopyFrom(JsonPreviewOptions);
            // Settings.Default.Save();
        }

        public void DiscardChanges()
        {
            // TODO
        }
        
    }*/
}