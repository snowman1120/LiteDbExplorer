using System.Windows;
using System.Windows.Media;

namespace LiteDbExplorer.Controls
{
    public static class StyleKit
    {
        public static Style MaterialDesignFlatButtonStyle => Application.Current.FindResource("MaterialDesignFlatButton") as Style;

        public static Style MaterialDesignEntryButtonStyle => Application.Current.FindResource("MaterialDesignEntryButton") as Style;

        public static SolidColorBrush MaterialDesignBody => Application.Current.FindResource("MaterialDesignBody") as SolidColorBrush;
    }
}