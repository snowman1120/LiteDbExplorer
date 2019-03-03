using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LiteDbExplorer.Presentation
{
    public static class IconProvider
    {
        public static Image GetImageIcon(string urlString, ImageIconOptions options = null)
        {
            if (options == null)
            {
                options = new ImageIconOptions();
            }

            var image = new Image
            {
                Source = new BitmapImage(new Uri(urlString, UriKind.Relative)),
                Stretch = options.Stretch,
            };

            if (options.Height.HasValue)
            {
                image.Height = options.Height.Value;
            }

            if (options.Width.HasValue)
            {
                image.Width = options.Width.Value;
            }

            RenderOptions.SetBitmapScalingMode(image, options.BitmapScalingMode);

            return image;
        }

    }

    public class ImageIconOptions
    {
        public double? Height { get; set; }
        public double? Width { get; set; }
        public Stretch Stretch { get; set; } = Stretch.Uniform;
        public BitmapScalingMode BitmapScalingMode { get; set; } = BitmapScalingMode.HighQuality;
    }
}