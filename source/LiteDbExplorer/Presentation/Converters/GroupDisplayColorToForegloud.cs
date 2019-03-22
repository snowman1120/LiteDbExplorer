using System.Globalization;
using System.Windows.Media;
using LiteDbExplorer.Modules.Main;
using LiteDbExplorer.Wpf.Converters;

namespace LiteDbExplorer.Presentation.Converters
{
    public class GroupDisplayColorToForegroundConverter : ConverterBase<string, SolidColorBrush>
    {
        public override SolidColorBrush Convert(string value, CultureInfo culture)
        {
            return GroupDisplayColor.GetDisplayColor(value, Colors.Gray);
        }
    }
}