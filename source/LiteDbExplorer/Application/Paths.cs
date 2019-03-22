using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace LiteDbExplorer
{
    public class Paths : INotifyPropertyChanged
    {

        private static readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new IgnoreParentPropertiesResolver(true),
            Formatting = Formatting.Indented
        };

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

        public static string LegacyRecentFilesPath => Path.Combine(AppDataPath, "recentfiles.txt");

        public static string RecentFilesPath => Path.Combine(AppDataPath, "recentfiles.json");

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

        
        public ObservableCollection<RecentFileInfo> RecentFiles => _lazyRecentFiles.Value;

        private readonly Lazy<ObservableCollection<RecentFileInfo>> _lazyRecentFiles = new Lazy<ObservableCollection<RecentFileInfo>>(() =>
        {
            var collection = new ObservableCollection<RecentFileInfo>();
            if (File.Exists(RecentFilesPath))
            {
                var value = File.ReadAllText(RecentFilesPath);
                if (!string.IsNullOrEmpty(value))
                {
                    var recentFileInfos = JsonConvert.DeserializeObject<RecentFileInfo[]>(value, _jsonSerializerSettings);
                    foreach (var recentFileInfo in recentFileInfos)
                    {
                        recentFileInfo.InvalidateInfo();
                        collection.Add(recentFileInfo);
                    }
                }
            }

            if (File.Exists(LegacyRecentFilesPath))
            {
                var filesPaths = File.ReadLines(LegacyRecentFilesPath);
                foreach (var filesPath in filesPaths)
                {
                    if (collection.Any(p => p.FullPath.Equals(filesPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    collection.Add(new RecentFileInfo(filesPath));
                }
            }
            
            collection.CollectionChanged += RecentFiles_CollectionChanged;

            return collection;

        });

        public void InsertRecentFile(string path)
        {
            var recentFileInfo = RecentFiles.FirstOrDefault(p => p.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase));
            if (recentFileInfo != null)
            {
                RecentFiles.Remove(recentFileInfo);
            }
            else
            {
                recentFileInfo = new RecentFileInfo(path);
            }

            recentFileInfo.LastOpenedAt = DateTime.Now;

            RecentFiles.Insert(0, recentFileInfo);
        }

        private static void RecentFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is ObservableCollection<RecentFileInfo> collection)
            {
                var json = JsonConvert.SerializeObject(collection, _jsonSerializerSettings);
                File.WriteAllText(RecentFilesPath, json);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RecentFileInfo
    {
        public RecentFileInfo()
        {
        }

        public RecentFileInfo(string fullPath)
        {
            FullPath = fullPath;
            InvalidateInfo();
        }

        public void InvalidateInfo()
        {
            FileName = Path.GetFileName(FullPath);
            DirectoryPath = Path.GetDirectoryName(FullPath);
            FileNotFound = !File.Exists(FullPath);
        }
        
        public string FullPath { get; set; }
        public DateTime? LastOpenedAt { get; set; }
        public bool Fixed { get; set; }

        [JsonIgnore]
        public string FileName { get; set; }

        [JsonIgnore]
        public string DirectoryPath { get; set; }

        [JsonIgnore]
        public bool FileNotFound { get; set; }
    }

}
