using System.ComponentModel.Composition;
using Caliburn.Micro;
using JetBrains.Annotations;
using LiteDbExplorer.Windows;
using LiteDB;

namespace LiteDbExplorer.Modules.Database
{
    [Export(typeof(IDatabasePropertiesView))]
    public class DatabasePropertiesViewModel : Screen, IDatabasePropertiesView
    {
        private LiteDatabase _database;
        private ushort _userVersion;

        public DatabasePropertiesViewModel()
        {
            DisplayName = "Database Properties";
        }

        public void Init(DatabaseReference database)
        {
            _database = database.LiteDatabase;

            DisplayName = $"Database Properties - {database.Name}";

            UserVersion = _database.Engine.UserVersion;
        }

        public ushort UserVersion
        {
            get => _userVersion;
            set
            {
                _userVersion = value;
                HasChanges = true;
            }
        }

        public bool HasChanges { get; private set; }

        public bool CanAcceptButton => HasChanges;

        public void AcceptButton()
        {
            if (_database.Engine.UserVersion != UserVersion)
            {
                _database.Engine.UserVersion = UserVersion;
            }

            TryClose(true);
        }

        public bool CanCancelButton => true;

        public void CancelButton()
        {
            TryClose(false);
        }

        public void ShrinkDatabase()
        {
            _database.Shrink();
        }
        
        [UsedImplicitly]
        public void SetPassword()
        {
            if (InputBoxWindow.ShowDialog("New password, enter empty string to remove password.", "", "", out string password) == true)
            {
                _database.Shrink(string.IsNullOrEmpty(password) ? null : password);
            }
        }
    }
}