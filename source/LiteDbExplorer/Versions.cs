using System;

namespace LiteDbExplorer
{
    public class Versions
    {
        public static Version CurrentVersion => System.Reflection.Assembly.GetEntryAssembly().GetName().Version;
    }
}
