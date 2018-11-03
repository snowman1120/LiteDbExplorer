using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace LiteDbExplorer
{
    public enum FieldSortOrder
    {
        Alphabetical,
        Original
    }

    public enum ColorTheme
    {
        Light,
        Dark
    }

    public class Settings : Freezable, INotifyPropertyChanged
    {
        private static readonly Lazy<Settings> _current =
            new Lazy<Settings>(Settings.LoadSettings);

        [Obsolete("This is a design-time only constructor, use static Current instead.")]
        public Settings()
        {
        }

        private Settings(DateTime timestamp)
        {
        }

        public static Settings Current => _current.Value;

        private Dictionary<string, WindowPosition> _windowPositions = new Dictionary<string, WindowPosition>();

        public Dictionary<string, WindowPosition> WindowPositions
        {
            get => _windowPositions;

            set
            {
                _windowPositions = value;
                OnPropertyChanged(nameof(WindowPositions));
            }
        }

        private FieldSortOrder _fieldSortOrder = FieldSortOrder.Original;

        public FieldSortOrder FieldSortOrder
        {
            get => _fieldSortOrder;

            set
            {
                _fieldSortOrder = value;
                OnPropertyChanged(nameof(FieldSortOrder));
            }
        }

        private double _mainSplitterSize = 250;
        public double MainSplitterSize
        {
            get => _mainSplitterSize;

            set
            {
                _mainSplitterSize = value;
                OnPropertyChanged(nameof(MainSplitterSize));
            }
        }

        private ColorTheme _colorTheme = ColorTheme.Light;
        public ColorTheme ColorTheme
        {
            get => _colorTheme;
            set
            {
                _colorTheme = value;
                OnPropertyChanged(nameof(ColorTheme));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private static Settings LoadSettings()
        {
            if (File.Exists(Paths.SettingsFilePath))
            {
                return JsonConvert.DeserializeObject<Settings>(File.ReadAllText(Paths.SettingsFilePath));
            }

            return new Settings(DateTime.UtcNow);
        }
        
        public void SaveSettings()
        {
            File.WriteAllText(Paths.SettingsFilePath, JsonConvert.SerializeObject(this, Formatting.Indented));
        }

        protected override Freezable CreateInstanceCore()
        {
            return Current;
        }

        public class WindowPosition
        {
            public class Point
            {
                public double X { get; set; }

                public double Y { get; set; }
            }

            public Point Position { get; set; }

            public Point Size { get; set; }
        }
    }
}