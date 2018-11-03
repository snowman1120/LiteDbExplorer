using System.Globalization;
using System.Windows.Media;
using LiteDbExplorer.Presentation.Behaviors;
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