using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using LiteDbExplorer.Annotations;

namespace LiteDbExplorer
{
    public class Paths : INotifyPropertyChanged
    {
        public static string AppDataPath
        {
            get
            {
                var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LiteDbExplorer");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        public static string ProgramFolder => Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);

        public static string UninstallerPath => Path.Combine(ProgramFolder, "uninstall.exe");

        public static string RecentFilesPath => Path.Combine(AppDataPath, "recentfiles.txt");

        public static string SettingsFilePath => Path.Combine(AppDataPath, "settings.json");

        public static string TempPath
        {
            get
            {
                var path = Path.Combine(Path.GetTempPath(), "LiteDbExplorer");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        
        public ObservableCollection<string> RecentFiles => _lazyRecentFiles.Value;

        private readonly Lazy<ObservableCollection<string>> _lazyRecentFiles = new Lazy<ObservableCollection<string>>(() =>
        {

            var collection = File.Exists(RecentFilesPath)
                ? new ObservableCollection<string>(File.ReadLines(RecentFilesPath))
                : new ObservableCollection<string>();
            
            collection.CollectionChanged += RecentFiles_CollectionChanged;

            return collection;

        });

        private static void RecentFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is ObservableCollection<string> collection)
            {
                File.WriteAllText(RecentFilesPath, string.Join(Environment.NewLine, collection));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
