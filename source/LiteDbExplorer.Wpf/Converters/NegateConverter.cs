using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LiteDbExplorer.Wpf.Converters
{
    /// <summary>
    /// Produces an output value that is the negative of the input.
    /// </summary>
    /// <remarks>
    /// The built-in signed types are supported as well as a handful of other commonly used types
    /// such as <see cref="T:Point"/>, <see cref="T:TimeSpan"/>,
    /// <see cref="T:Thickness"/>, etc.
    /// </remarks>
    /// <seealso cref="T:System.Windows.Data.IValueConverter"/>
    public sealed class NegateConverter : IValueConverter
    {

        #region Fields

        /// <summary>
        /// The default singleton instance of this converter.
        /// </summary>
        public static readonly NegateConverter Default = new NegateConverter();

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="T:NegateConverter"/> class.
        /// </summary>
        public NegateConverter()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <exception cref="ArgumentException"> Thrown when one or more arguments have unsupported or
        /// illegal values.</exception>
        /// <param name="value">      The value produced by the binding source.</param>
        /// <param name="targetType"> The type of the binding target property.</param>
        /// <param name="parameter">  The converter parameter to use.</param>
        /// <param name="culture">    The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <seealso cref="M:System.Windows.Data.IValueConverter.Convert(object,Type,object,CultureInfo)"/>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value == null)
            {
                return null;
            }

            if (value is double d)
            {
                return Negate(d);
            }

            if (value is int i)
            {
                return Negate(i);
            }

            if (value is bool b)
            {
                return Negate(b);
            }

            if (value is long l)
            {
                return Negate(l);
            }

            if (value is IConvertible convertible)
            {
                return Negate(convertible, culture);
            }

            if (value is Visibility visibility)
            {
                return Negate(visibility);
            }

            if (value is TimeSpan span)
            {
                return Negate(span);
            }

            if (value is Point point)
            {
                return Negate(point);
            }

            if (value is Thickness thickness)
            {
                return Negate(thickness);
            }

            throw new ArgumentException($"Cannot negate {value.GetType()}.", "value");

        }
        
        /// <summary>
        /// Converts a value.
        /// </summary>
        /// <param name="value">      The value that is produced by the binding target.</param>
        /// <param name="targetType"> The type to convert to.</param>
        /// <param name="parameter">  The converter parameter to use.</param>
        /// <param name="culture">    The culture to use in the converter.</param>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <seealso cref="M:System.Windows.Data.IValueConverter.ConvertBack(object,Type,object,CultureInfo)"/>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Convert(value, targetType, parameter, culture);
        }

        private Visibility Negate(Visibility visibility)
        {
            return visibility == Visibility.Hidden || visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }

        private static TimeSpan Negate(TimeSpan value)
        {
            return value.Negate();
        }

        private static Point Negate(Point value)
        {
            return new Point(
                -value.X,
                -value.Y
            );
        }

        private static Thickness Negate(Thickness value)
        {
            return new Thickness(
                -value.Left,
                -value.Top,
                -value.Right,
                -value.Bottom
            );
        }

        private static bool Negate(bool value)
        {
            return !value;
        }

        private static int Negate(int value)
        {
            return -value;
        }

        private static long Negate(long value)
        {
            return -value;
        }

        private static double Negate(double value)
        {
            return -value;
        }

        private static object Negate(IConvertible value, IFormatProvider formatProvider)
        {

            TypeCode inputType = value.GetTypeCode();

            decimal input = value.ToDecimal(formatProvider);
            decimal output = Decimal.Negate(input);

            return System.Convert.ChangeType(output, inputType, formatProvider);

        }

        #endregion

    }
}