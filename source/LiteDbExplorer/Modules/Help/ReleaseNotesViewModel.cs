using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Primitives;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using LiteDbExplorer.Framework.Windows;
using Octokit;

namespace LiteDbExplorer.Modules.Help
{
    [Export(typeof(ReleaseNotesViewModel))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    public class ReleaseNotesViewModel : Screen
    {
        public static DialogOptions DefaultDialogOptions = new DialogOptions
        {
            Height = 570,
            Width = 765,
            ResizeMode = System.Windows.ResizeMode.NoResize,
            SizeToContent = System.Windows.SizeToContent.Manual,
            ShowMinButton = false,
            ShowMaxRestoreButton = false,
            ShowIconOnTitleBar = false,
            ShowDialogsOverTitleBar = true,
            ShowInTaskbar = false
        };

        private Version _filerVersion;

        public ReleaseNotesViewModel()
        {
            DisplayName = "Release Notes";
        }

        public bool IsBusy { get; private set; }

        public string EmptyMessage { get; private set; }

        public bool ShowEmptyMessage => !string.IsNullOrEmpty(EmptyMessage);

        public ICollection<ReleaseNoteItem> ReleaseNotes { get; private set; }

        public void FilterVersion(Version version)
        {
            _filerVersion = version;
            ReleaseNotes = null;
        }

        protected override async void OnActivate()
        {
            if (ReleaseNotes == null)
            {
                await GetReleaseNotes().ConfigureAwait(false);
            }
        }

        public async Task GetReleaseNotes()
        {
            IsBusy = true;

            try
            {
                var client = new GitHubClient(new ProductHeaderValue(AppConstants.Github.RepositoryOwner));

                var releases = await client.Repository.Release.GetAll(AppConstants.Github.RepositoryOwner,
                    AppConstants.Github.RepositoryName);

                var releaseNotes = releases
                    .Select(p => new ReleaseNoteItem
                    {
                        Name = p.Name,
                        CreatedAt = p.CreatedAt,
                        TagName = p.TagName,
                        Content = p.Body,
                        Version = Version.TryParse(p.TagName, out var version) ? version : null
                    })
                    .ToList();

                if (_filerVersion != null)
                {
                    releaseNotes = releaseNotes.Where(p => _filerVersion.Equals(p.Version)).ToList();
                }

                foreach (var releaseNote in releaseNotes)
                {
                    releaseNote.IsCurrent = Versions.CurrentVersion.Equals(releaseNote.Version);
                }

                ReleaseNotes = releaseNotes;

                if (!ReleaseNotes.Any())
                {
                    EmptyMessage = "No release notes.";
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                EmptyMessage = "Unable to check Release Notes.";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    public class ReleaseNoteItem
    {
        public string Name { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string TagName { get; set; }
        public Version Version { get; set; }
        public bool IsCurrent { get; set; }
        public string Content { get; set; }
    }
}