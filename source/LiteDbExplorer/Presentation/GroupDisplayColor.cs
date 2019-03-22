using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace LiteDbExplorer.Presentation
{
    public class GroupDisplayColor
    {
        public static readonly string[] GroupDisplayColorsSet = {"#8DA3C1","#9D827B","#C1AA66","#869A87","#C97E6C","#617595","#846A62","#887E5C","#607562","#BA5E41","#3D5573","#694F47","#696658","#425E45","#8D4823"};
        public static readonly IDictionary<string, SolidColorBrush> GroupDisplayColors = new Dictionary<string, SolidColorBrush>();

        public static SolidColorBrush GetDisplayColor(string groupName, Color emptyColor)
        {
            if (string.IsNullOrEmpty(groupName))
            {
                return new SolidColorBrush(emptyColor);
            }

            if (GroupDisplayColors.ContainsKey(groupName))
            {
                return GroupDisplayColors[groupName];
            }

            var count = GroupDisplayColorsSet.Length;
            var colors = GroupDisplayColors.Count;
            var colorIndex = Math.Max(0, (count) % (colors + 1));

            var groupDisplayColorHexa = GroupDisplayColorsSet[colorIndex];
            var solidColorBrush = (SolidColorBrush) new BrushConverter().ConvertFrom(groupDisplayColorHexa);
            GroupDisplayColors.Add(groupName, solidColorBrush);
            return solidColorBrush;
        }
    }
}