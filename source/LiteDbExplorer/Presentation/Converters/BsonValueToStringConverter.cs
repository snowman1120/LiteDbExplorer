using System;
using System.Windows.Data;
using LiteDbExplorer.Core;
using LiteDbExplorer.Modules;
using LiteDB;

namespace LiteDbExplorer.Presentation.Converters
{
    class BsonValueToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null && value is BsonValue bsonValue)
            {
                return bsonValue.ToDisplayValue();
            }

            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            return new BsonValue(value as string);
        }
    }
}
