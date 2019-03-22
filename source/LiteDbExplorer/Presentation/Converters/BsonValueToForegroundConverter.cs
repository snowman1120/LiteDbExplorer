using System.Globalization;
using System.Windows.Media;
using LiteDbExplorer.Presentation.Behaviors;
using LiteDbExplorer.Wpf.Converters;
using LiteDB;

namespace LiteDbExplorer.Presentation.Converters
{
    public class BsonValueToForegroundConverter : ConverterBase<BsonValue, SolidColorBrush>
    {
        public override SolidColorBrush Convert(BsonValue value, CultureInfo culture)
        {
            return BsonValueForeground.GetBsonValueForeground(value);
        }
    }
}