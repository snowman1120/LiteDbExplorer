using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Caliburn.Micro;
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

        
        public IObservableCollection<RecentFileInfo> RecentFiles => _lazyRecentFiles.Value;

        private readonly Lazy<BindableCollection<RecentFileInfo>> _lazyRecentFiles = new Lazy<BindableCollection<RecentFileInfo>>(() =>
        {
            var list = new List<RecentFileInfo>();
            var recentFilesExists = File.Exists(RecentFilesPath);
            if (recentFilesExists)
            {
                var value = File.ReadAllText(RecentFilesPath);
                if (!string.IsNullOrEmpty(value))
                {
                    var recentFileInfos = JsonConvert.DeserializeObject<RecentFileInfo[]>(value, _jsonSerializerSettings);
                    foreach (var recentFileInfo in recentFileInfos)
                    {
                        recentFileInfo.InvalidateInfo();
                        list.Add(recentFileInfo);
                    }
                }
            }

            if (File.Exists(LegacyRecentFilesPath) && !recentFilesExists)
            {
                var filesPaths = File.ReadLines(LegacyRecentFilesPath);
                foreach (var filesPath in filesPaths)
                {
                    if (list.Any(p => p.FullPath.Equals(filesPath, StringComparison.OrdinalIgnoreCase)))
                    {
                        continue;
                    }

                    list.Add(new RecentFileInfo(filesPath));
                }
            }
            
            var collection = new BindableCollection<RecentFileInfo>(list);

            ReorderRecentFiles(collection);

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

            RecentFiles.IsNotifying = false;
            RecentFiles.Insert(0, recentFileInfo);
            RecentFiles.IsNotifying = true;

            ReorderRecentFiles(RecentFiles);
        }

        public bool RemoveRecentFile(string path)
        {
            var recentFileInfo = RecentFiles.FirstOrDefault(p => p.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase));
            if (recentFileInfo != null)
            {
                return RecentFiles.Remove(recentFileInfo);
            }

            return false;
        }

        public void SetRecentFileFixed(string path, bool add)
        {
            var recentFileInfo = RecentFiles.FirstOrDefault(p => p.FullPath.Equals(path, StringComparison.OrdinalIgnoreCase));
            if (recentFileInfo != null)
            {
                recentFileInfo.FixedAt = add ? DateTime.Now : (DateTime?) null;
                
                ReorderRecentFiles(RecentFiles);
            }
        }

        public static void ReorderRecentFiles(IObservableCollection<RecentFileInfo> target)
        {
            if (target == null)
            {
                return;
            }
            
            var orderedItem = new List<RecentFileInfo>();
            orderedItem.AddRange(target.Where(p => p.FixedAt.HasValue).OrderByDescending(p => p.FixedAt));
            orderedItem.AddRange(target.Where(p => !p.FixedAt.HasValue).OrderByDescending(p => p.LastOpenedAt));

            target.IsNotifying = false;

            target.Clear();

            target.IsNotifying = true;
            
            target.AddRange(orderedItem);
        }

        private static void RecentFiles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is IObservableCollection<RecentFileInfo> collection)
            {
                var json = JsonConvert.SerializeObject(collection, _jsonSerializerSettings);
                
                File.WriteAllText(RecentFilesPath, json);

                if (File.Exists(LegacyRecentFilesPath))
                {
                    File.WriteAllLines(LegacyRecentFilesPath, collection.Select(p => p.FullPath));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    
    public class RecentFileInfo : INotifyPropertyChanged
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
        public DateTime? FixedAt { get; set; }

        [JsonIgnore]
        public string FileName { get; set; }

        [JsonIgnore]
        public string DirectoryPath { get; set; }

        [JsonIgnore]
        public bool? FileNotFound { get; set; }

        [JsonIgnore]
        public bool IsFixed => FixedAt.HasValue;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
