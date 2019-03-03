using System;
using System.Globalization;
using System.Windows.Media;

namespace LiteDbExplorer.Presentation.Converters
{
    public class BrushToContrastForegroundConverter : ConverterBase<SolidColorBrush, SolidColorBrush>
    {
        public Color FallbackColor { get; set; } = Colors.Black;

        public override SolidColorBrush Convert(SolidColorBrush value, CultureInfo culture)
        {
            if (value == null)
            {
                return new SolidColorBrush(FallbackColor);
            }

            var idealTextColor = IdealTextColor(value.Color);

            return  new SolidColorBrush(idealTextColor);
        }
        

        public static Color IdealTextColor(Color bg)
        {
            int nThreshold = 105;
            int bgDelta = System.Convert.ToInt32((bg.R * 0.299) + (bg.G * 0.587) + (bg.B * 0.114));

            Color foreColor = (255 - bgDelta < nThreshold) ? Colors.Black : Colors.White;
            return foreColor;
        }

        
    }
}