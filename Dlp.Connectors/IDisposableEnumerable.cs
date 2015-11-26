using System;
using System.Collections.Generic;

namespace Dlp.Connectors
{
    public interface IDisposableEnumerable<T> : IEnumerable<T>, IDisposable { }
}
