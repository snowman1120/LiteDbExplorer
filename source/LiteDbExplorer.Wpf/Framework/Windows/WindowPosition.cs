namespace LiteDbExplorer.Wpf.Framework.Windows
{
    public class WindowPosition
    {
        public class Point
        {
            public double X { get; set; }

            public double Y { get; set; }
        }

        public Point Position { get; set; }

        public Point Size { get; set; }
    }
}