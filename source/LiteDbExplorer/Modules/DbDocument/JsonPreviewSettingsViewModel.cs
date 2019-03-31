using System.ComponentModel.Composition;
using Caliburn.Micro;
using LiteDbExplorer.Controls.Editor;
using LiteDbExplorer.Wpf.Modules.Settings;

namespace LiteDbExplorer.Modules.DbDocument
{
    [Export(typeof(ISettingsEditor))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class JsonPreviewSettingsViewModel : PropertyChangedBase, ISettingsEditor, IAutoGenSettingsView
    {
        public JsonPreviewSettingsViewModel()
        {
            // JsonPreviewOptions = new ExtendedTextEditorOptions(Settings.Default.TextEditor_JsonPreviewOptions);
            JsonPreviewOptions = new ExtendedTextEditorOptions();
        }
        
        public string SettingsPagePath => "Document Preview";

        public string SettingsPageName => "Json Editor";

        public int EditorDisplayOrder => 10;

        public string DisplayName => string.Empty;

        public object AutoGenContext => JsonPreviewOptions;
        
        public void ResetDefault()
        {
            JsonPreviewOptions = new ExtendedTextEditorOptions();
            NotifyOfPropertyChange(() => JsonPreviewOptions);
        }

        public ExtendedTextEditorOptions JsonPreviewOptions { get; private set; }

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
        
    }
}