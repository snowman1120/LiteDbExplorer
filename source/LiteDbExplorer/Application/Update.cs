using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;

namespace LiteDbExplorer
{
    public class Update
    {
        public class UpdateData
        {
            public string version
            {
                get; set;
            }

            public string url
            {
                get; set;
            }
        }

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private UpdateData _latestData;
        private readonly string _updaterPath = Path.Combine(Paths.TempPath, "update.exe");
        private readonly string _downloadCompletePath = Path.Combine(Paths.TempPath, "download.done");

        public bool IsUpdateAvailable => GetLatestVersion().CompareTo(Versions.CurrentVersion) > 0;

        public void DownloadUpdate()
        {
            if (_latestData == null)
            {
                GetLatestVersion();
            }

            DownloadUpdate(_latestData.url);
        }

        public void DownloadUpdate(string url)
        {
            Logger.Info("Downloading new update from " + url);
            Directory.CreateDirectory(Paths.TempPath);

            if (File.Exists(_downloadCompletePath) && File.Exists(_updaterPath))
            {                
                var info = FileVersionInfo.GetVersionInfo(_updaterPath);
                if (info.FileVersion == GetLatestVersion().ToString())
                {
                    Logger.Info("Update already ready to install");
                    return;
                }

                File.Delete(_downloadCompletePath);
            }

            (new WebClient()).DownloadFile(url, _updaterPath);
            File.Create(_downloadCompletePath);
        }

        public void InstallUpdate()
        {
            var portable = Config.IsPortable ? "/Portable 1" : "/Portable 0";
            Logger.Info("Installing new update to {0}, in {1} mode", Paths.ProgramFolder, portable);
            Task.Factory.StartNew(() =>
            {
                Process.Start(_updaterPath, string.Format(@"/ProgressOnly 1 {0} /D={1}", portable, Paths.ProgramFolder));
            });

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (Application.Current.MainWindow != null) 
                    Application.Current.MainWindow.Close();
            });
        }

        public Version GetLatestVersion()
        {
            var dataString = (new WebClient()).DownloadString(Config.UpdateDataUrl);
            _latestData = JsonConvert.DeserializeObject<Dictionary<string, UpdateData>>(dataString)["stable"];
            return new Version(_latestData.version);
        }
    }
}
