using System.Windows;
using System.Windows.Media;

namespace LiteDbExplorer.Controls
{
    public static class StyleKit
    {
        public static Style MaterialDesignFlatButtonStyle => Application.Current.FindResource("MaterialDesignFlatButton") as Style;

        public static Style MaterialDesignEntryButtonStyle => Application.Current.FindResource("MaterialDesignEntryButton") as Style;

        public static SolidColorBrush MaterialDesignBody => Application.Current.FindResource("MaterialDesignBody") as SolidColorBrush;

        public static SolidColorBrush MaterialDesignBodyLight => Application.Current.FindResource("MaterialDesignBodyLight") as SolidColorBrush;

        public static SolidColorBrush MaterialDesignBackground => Application.Current.FindResource("MaterialDesignBackground") as SolidColorBrush;

        public static SolidColorBrush MaterialDesignCardBackground => Application.Current.FindResource("MaterialDesignCardBackground") as SolidColorBrush;
        
        public static SolidColorBrush MaterialDesignPaper => Application.Current.FindResource("MaterialDesignPaper") as SolidColorBrush;
        
        public static SolidColorBrush PrimaryHueMidBrush => Application.Current.FindResource("PrimaryHueMidBrush") as SolidColorBrush;

        public static SolidColorBrush PrimaryHueDarkBrush => Application.Current.FindResource("PrimaryHueDarkBrush") as SolidColorBrush;

        public static SolidColorBrush AccentColorBrush => Application.Current.FindResource("AccentColorBrush") as SolidColorBrush;

        public static SolidColorBrush AccentColorBrush2 => Application.Current.FindResource("AccentColorBrush2") as SolidColorBrush;

        public static SolidColorBrush AccentColorBrush3 => Application.Current.FindResource("AccentColorBrush3") as SolidColorBrush;
        
        public static SolidColorBrush SecondaryAccentBrush => Application.Current.FindResource("SecondaryAccentBrush") as SolidColorBrush;

        public static SolidColorBrush SecondaryAccentForegroundBrush => Application.Current.FindResource("SecondaryAccentForegroundBrush") as SolidColorBrush;
    }
}