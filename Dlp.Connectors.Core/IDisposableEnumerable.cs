using System;
using System.Collections.Generic;

namespace Dlp.Connectors.Core {

    public interface IDisposableEnumerable<T> : IEnumerable<T>, IDisposable { }
}
