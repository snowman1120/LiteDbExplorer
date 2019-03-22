using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using LiteDbExplorer.Wpf.Framework.Windows;
using Newtonsoft.Json.Serialization;

namespace LiteDbExplorer
{
    public enum ColorTheme
    {
        Light,
        Dark
    }

    public partial class Settings : Freezable, INotifyPropertyChanged, IWindowStateStore
    {
        private static readonly Lazy<Settings> _current =
            new Lazy<Settings>(Settings.LoadSettings);

        private static readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            ContractResolver = new IgnoreParentPropertiesResolver(true),
            Formatting = Formatting.Indented
        };

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
                var value = File.ReadAllText(Paths.SettingsFilePath);
                return JsonConvert.DeserializeObject<Settings>(value, _settings);
            }

            return new Settings(DateTime.UtcNow);
        }

        public void SaveSettings()
        {
            File.WriteAllText(Paths.SettingsFilePath, JsonConvert.SerializeObject(this, _settings));
        }

        protected override Freezable CreateInstanceCore()
        {
            return Current;
        }
    }

    public class IgnoreParentPropertiesResolver : DefaultContractResolver
    {
        readonly bool _ignoreBase = false;

        public IgnoreParentPropertiesResolver(bool ignoreBase)
        {
            _ignoreBase = ignoreBase;
        }

        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var allProps = base.CreateProperties(type, memberSerialization);
            if (!_ignoreBase) return allProps;

            //Choose the properties you want to serialize/deserialize
            var props = type.GetProperties(~BindingFlags.FlattenHierarchy);

            return allProps.Where(p => props.Any(a => a.Name == p.PropertyName)).ToList();
        }
    }
}