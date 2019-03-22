using System;
using System.Windows.Media;
using LiteDbExplorer.Controls;
using LiteDbExplorer.Wpf;

namespace LiteDbExplorer.Presentation
{
    public static class ThemeManager
    {
        public static event EventHandler CurrentThemeChanged;

        public static void InitColorTheme(ColorTheme colorTheme)
        {
            new LocalPaletteHelper().InitTheme(colorTheme == ColorTheme.Dark);

            OnThemeChange();
        }

        public static void SetColorTheme(ColorTheme colorTheme)
        {
            new LocalPaletteHelper().SetLightDark(colorTheme == ColorTheme.Dark);

            OnThemeChange();
        }

        private static void OnThemeChange()
        {
            CurrentThemeChanged?.Invoke(null, EventArgs.Empty);
        }

        public class TypeHighlighting
        {
            public static SolidColorBrush ObjectForeground => GetSolidColorBrush("#707070");

            public static SolidColorBrush NumberForeground => GetSolidColorBrush("#2882F9");

            public static SolidColorBrush BooleanForeground => GetSolidColorBrush("#1F61A0");

            public static SolidColorBrush DateTimeForeground => GetSolidColorBrush("#1c92a9");

            public static SolidColorBrush StringForeground => GetSolidColorBrush("#D64292");

            public static SolidColorBrush Default => StyleKit.MaterialDesignBody;
        }

        public static SolidColorBrush GetSolidColorBrush(string hex)  
        {
            return new BrushConverter().ConvertFrom(hex) as SolidColorBrush;
        }
        
    }
}