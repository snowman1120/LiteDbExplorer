using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using LiteDB;

namespace LiteDbExplorer.Converters
{
    public class BsonValueToForegroundConverter : IValueConverter
    {
        public SolidColorBrush Default { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is BsonValue bsonValue)
            {
                if (bsonValue.IsDocument || bsonValue.IsArray || bsonValue.IsBinary)
                {
                    return new BrushConverter().ConvertFrom("#707070");
                }
                
                if (bsonValue.IsInt32 || bsonValue.IsInt64 || bsonValue.IsDecimal || bsonValue.IsDouble)
                {
                    return new BrushConverter().ConvertFrom("#2882F9");
                }

                if (bsonValue.IsBoolean)
                {
                    return new BrushConverter().ConvertFrom("#1F61A0");
                }

                if (bsonValue.IsDateTime)
                {
                    return new BrushConverter().ConvertFrom("#1c92a9");
                }

                if (bsonValue.IsString)
                {
                    return new BrushConverter().ConvertFrom("#D64292");
                }
            }
            

            return Default;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}