using System;
using System.Collections.Generic;

namespace LiteDbExplorer.Framework.Services
{
    public interface IFileDropSource
    {
        Action<IEnumerable<string>> FilesDropped { get; set; }
    }
}