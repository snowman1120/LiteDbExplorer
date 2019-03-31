using Caliburn.Micro;

namespace LiteDbExplorer.Wpf.Modules.Settings
{
    public interface IAutoGenSettingsView
    {
        string GroupDisplayName { get; }
        object AutoGenContext { get; }
    }

    public interface ISettingsEditor
    {
        string SettingsPageName { get; }
        string SettingsPagePath { get; }
        int EditorDisplayOrder { get; }

        void ApplyChanges();
        void DiscardChanges();
    }
}