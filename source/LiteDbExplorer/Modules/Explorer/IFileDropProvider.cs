using System;
using System.Collections.Generic;

namespace LiteDbExplorer.Modules.Explorer
{
    public interface IFileDropSource
    {
        Action<IEnumerable<string>> FilesDropped { get; set; }
    }
}