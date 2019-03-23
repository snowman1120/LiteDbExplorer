using System.Globalization;
using System.Windows;

namespace LiteDbExplorer.Wpf.Converters
{
    public class BooleanToVisibilityConverter : ConverterBase<bool, Visibility>
    {
        public BooleanToVisibilityConverter()
        {
            TrueValue = Visibility.Visible;
            FalseValue = Visibility.Collapsed;
        }

        public Visibility TrueValue { get; set; }
        public Visibility FalseValue { get; set; }
        public bool Inverse { get; set; }

        public override Visibility Convert(bool value, CultureInfo culture)
        {
            if (Inverse)
            {
                return value ? FalseValue : TrueValue;
            }

            return value ? TrueValue : FalseValue;
        }
    }
}