using System.Windows;

namespace LiteDbExplorer.Controls
{
    public static class StyleKit
    {
        public static Style MaterialDesignFlatButtonStyle => Application.Current.FindResource("MaterialDesignFlatButton") as Style;

        public static Style MaterialDesignEntryButtonStyle => Application.Current.FindResource("MaterialDesignEntryButton") as Style;
    }
}