using System;
using System.Windows.Data;
using LiteDB;

namespace LiteDbExplorer.Presentation.Converters
{
    public class BsonValueToNetValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value is BsonValue bsonValue)
            {
                return bsonValue.RawValue;
            }

            throw new Exception("Cannot convert non BSON value");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            return new BsonValue(value);
        }
    }
}
