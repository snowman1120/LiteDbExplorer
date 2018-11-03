using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LiteDbExplorer.Presentation.Converters
{
    public class DoubleToGridLengthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (double)value;
            return new GridLength(val);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (GridLength)value;
            return val.Value;
        }
    }
}
