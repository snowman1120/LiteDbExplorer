using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LiteDbExplorer.Presentation.Converters
{
    public class NullOrEmptyToVisibilityConverter : ConverterBase<object, Visibility>
    {
        public NullOrEmptyToVisibilityConverter()
        {
            TrueValue = Visibility.Collapsed;
            FalseValue = Visibility.Visible;
        }

        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }
        public bool SeeDefaultTypeValue { get; set; }
        public bool SeeEmpty { get; set; }

        public override Visibility Convert(object value, CultureInfo culture)
        {
            var result = false;

            if (value == null)
                return TrueValue;

            if (SeeDefaultTypeValue)
            {
                var type = value.GetType();
                result = value == GetDefaultValue(type);
            }

            if (SeeEmpty && value is string s)
            {
                result = result || string.IsNullOrEmpty(s);
            }

            if (result)
            {
                return TrueValue;
            }

            return FalseValue;
        }

        private static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t);
            }

            return null;
        }
    }

    /// <summary>
    /// Null or empty value to boolean converter. If null return true value
    /// </summary>
    /// <seealso cref="T:System.Windows.Data.IValueConverter"/>
    public class NullOrEmptyValueToBooleanConverter : IValueConverter
    {
        public NullOrEmptyValueToBooleanConverter()
        {
            TrueValue = true;
            FalseValue = false;
            SeeDefaultTypeValue = true;
            SeeEmpty = true;
        }

        public bool TrueValue { get; set; }
        public bool FalseValue { get; set; }
        public bool SeeDefaultTypeValue { get; set; }
        public bool SeeEmpty { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = false;

            if (value == null)
                return TrueValue;

            if (SeeDefaultTypeValue)
            {
                Type type = value.GetType();
                result = value == GetDefaultValue(type);
            }

            if (SeeEmpty && value is string s)
            {
                result = result || string.IsNullOrEmpty(s);
            }

            if (result)
            {
                return TrueValue;
            }

            return FalseValue;

        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static object GetDefaultValue(Type t)
        {
            if (t.IsValueType)
            {
                return Activator.CreateInstance(t);
            }

            return null;
        }
    }
}