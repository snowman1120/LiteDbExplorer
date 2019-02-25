using System;
using System.Windows.Data;
using LiteDB;

namespace LiteDbExplorer.Presentation.Converters
{
    class BsonValueToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            if (value is BsonValue bsonValue)
            {
                try
                {
                    if (bsonValue.IsDocument)
                    {
                        return "[Document]";
                    }
                    if (bsonValue.IsArray)
                    {
                        return "[Array]";
                    }
                    if (bsonValue.IsBinary)
                    {
                        return "[Binary]";
                    }
                    if (bsonValue.IsObjectId)
                    {
                        return bsonValue.AsString;
                    }
                    if (bsonValue.IsDateTime)
                    {
                        return bsonValue.AsDateTime;
                    }
                    if (bsonValue.IsInt32)
                    {
                        return bsonValue.AsInt32;
                    }
                    if (bsonValue.IsInt64)
                    {
                        return bsonValue.AsInt64;
                    }
                    if (bsonValue.IsDouble)
                    {
                        return bsonValue.AsDouble;
                    }
                    if (bsonValue.IsDecimal)
                    {
                        return bsonValue.AsDecimal;
                    }
                    if (bsonValue.IsGuid)
                    {
                        return bsonValue.AsGuid;
                    }
                    if (bsonValue.IsString)
                    {
                        return bsonValue.AsString;
                    }

                    return bsonValue.ToString();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return string.Empty;
                }
                
            }

            throw new Exception("Cannot convert non BSON value");
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
