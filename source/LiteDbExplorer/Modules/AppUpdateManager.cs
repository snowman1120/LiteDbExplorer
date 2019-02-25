using System;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using JetBrains.Annotations;
using LiteDbExplorer.Framework;
using Onova;
using Onova.Services;

namespace LiteDbExplorer.Modules
{
    public class AppUpdateManager : Freezable, INotifyPropertyChanged
    {
        private readonly UpdateManager _updateManager;
        private bool _hasUpdate;
        private bool _isBusy;

        private static readonly Lazy<AppUpdateManager> _instance =
            new Lazy<AppUpdateManager>(() => new AppUpdateManager());

        public static AppUpdateManager Current => _instance.Value;

        public AppUpdateManager()
        {
            _updateManager = new UpdateManager(
                new GithubPackageResolver(@"julianpaulozzi", "LiteDbExplorer", "*.zip"), 
                new LocalZipPackageExtractor());

            UpdateActionText = "Update";

            CheckForUpdatesCommand = new RelayCommand(async _ => await CheckForUpdates(true), _ => !IsBusy);

            DoUpdateCommand = new RelayCommand(async _ => await DoUpdate(), _ => HasUpdate && !IsBusy);
        }

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                _isBusy = value;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool HasUpdate
        {
            get => _hasUpdate;
            private set
            {
                _hasUpdate = value;
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string UpdateActionText { get; private set; }

        public string UpdateMessage { get; private set; }

        public Version LastVersion { get; private set; }

        public bool IsUpdatePrepared { get; private set; }

        public double UpdateProgress { get; private set; }

        public ICommand CheckForUpdatesCommand { get; private set; }

        public ICommand DoUpdateCommand { get; private set; }
        
        public async Task CheckForUpdates(bool userInitiated)
        {
            IsBusy = true;
            HasUpdate = false;
            UpdateMessage = string.Empty;
            LastVersion = null;
            try
            {
                // Check for updates
                var result = await _updateManager.CheckForUpdatesAsync();

                if (result.CanUpdate)
                {
                    HasUpdate = true;
                    UpdateMessage = $"A new version {result.LastVersion} is available for update.";

                    LastVersion = result.LastVersion;

                    if (userInitiated && MessageBox.Show(
                            UpdateMessage + "\n\nDownload and update now?",
                            "Update available.",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question
                        ) == MessageBoxResult.Yes)
                    {
                        await DoUpdate();
                    }

                    IsBusy = false;

                    return;
                }

                if (userInitiated)
                {
                    MessageBox.Show("There are currently no updates available.",
                        $"LiteDB Explorer {Versions.CurrentVersion}");
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                if (userInitiated)
                {
                    MessageBox.Show("Unable to check for updates.", $"LiteDB Explorer {Versions.CurrentVersion}");
                }
            }

            HasUpdate = false;
            IsBusy = false;
        }

        public async Task DoUpdate()
        {
            if (LastVersion == null)
            {
                return;
            }

            IsUpdatePrepared = _updateManager.IsUpdatePrepared(LastVersion);
            if (IsUpdatePrepared)
            {
                // Launch an executable that will apply the update
                // (can optionally restart application on completion)
                _updateManager.LaunchUpdater(LastVersion, true);

                // External updater will wait until the application exits
                Environment.Exit(0);
                return;
            }

            IsBusy = true;
            UpdateProgress = 0;

            try
            {
                var cancellationTokenSource = new CancellationTokenSource();

                UpdateActionText = "Downloading...";

                // Prepare an update so it can be applied later
                // (supports optional progress reporting and cancellation)
                await _updateManager.PrepareUpdateAsync(LastVersion,
                    new Progress<double>(val =>
                    {
                        var progress = val > 90 ? "Preparing" : "Downloading";
                        UpdateMessage = $"{progress}... {val:P0}";
                        UpdateProgress = val;
                    }),
                    cancellationTokenSource.Token);

                IsUpdatePrepared = _updateManager.IsUpdatePrepared(LastVersion);

                UpdateActionText = "Restart";
                UpdateMessage = "Ready, restart to update...";
                UpdateProgress = 100;

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                UpdateActionText = "Update";
                UpdateMessage = "Error on application update.";
                MessageBox.Show(
                    e.Message,
                    "Error on application update.",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }

            IsBusy = false;
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override Freezable CreateInstanceCore()
        {
            return _instance.Value;
        }
    }

    /// <summary>
    /// Extracts files from zip-archived packages.
    /// </summary>
    public class LocalZipPackageExtractor : IPackageExtractor
    {
        /// <inheritdoc />
        public async Task ExtractAsync([NotNull] string sourceFilePath, [NotNull] string destDirPath,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (sourceFilePath == null)
            {
                throw new ArgumentNullException(nameof(sourceFilePath));
            }

            if (destDirPath == null)
            {
                throw new ArgumentNullException(nameof(destDirPath));
            }


            // Read the zip
            using (var archive = ZipFile.OpenRead(sourceFilePath))
            {
                // For progress reporting
                var totalBytes = archive.Entries.Sum(e => e.Length);
                var totalBytesCopied = 0L;

                // Loop through all entries
                foreach (var entry in archive.Entries)
                {
                    if (entry.Length == 0)
                    {
                        continue;
                    }

                    // Get destination paths
                    var entryDestFilePath = Path.Combine(destDirPath, entry.FullName);
                    var entryDestDirPath = Path.GetDirectoryName(entryDestFilePath);

                    // Create directory
                    Directory.CreateDirectory(entryDestDirPath);

                    // Extract entry
                    using (var input = entry.Open())
                    using (var output = File.Create(entryDestFilePath))
                    {
                        int bytesCopied;
                        do
                        {
                            // Copy
                            bytesCopied = await CopyChunkToAsync(input, output, cancellationToken)
                                .ConfigureAwait(false);

                            // Report progress
                            totalBytesCopied += bytesCopied;
                            progress?.Report(1.0 * totalBytesCopied / totalBytes);
                        } while (bytesCopied > 0);
                    }
                }
            }
        }

        public static async Task<int> CopyChunkToAsync(Stream source, Stream destination,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            var buffer = new byte[81920];

            // Read
            var bytesCopied = await source.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);

            // Write
            await destination.WriteAsync(buffer, 0, bytesCopied, cancellationToken).ConfigureAwait(false);

            return bytesCopied;
        }
    }
}