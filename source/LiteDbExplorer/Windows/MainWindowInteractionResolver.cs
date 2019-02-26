using System.Windows;
using LiteDbExplorer.Modules;
using LiteDbExplorer.Windows;
using LiteDB;

namespace LiteDbExplorer
{
    public class MainWindowInteractionResolver : IInteractionResolver
    {
        public Window Owner { get; set; }

        public void ShowDatabaseProperties(LiteDatabase database)
        {
            var window = new DatabasePropertiesWindow(Store.Current.SelectedDatabase.LiteDatabase)
            {
                Owner = Owner
            };

            window.ShowDialog();    
        }
    }
}