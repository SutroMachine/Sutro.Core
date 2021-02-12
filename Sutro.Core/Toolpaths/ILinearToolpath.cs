using System.Collections.Generic;

namespace Sutro.Core.Toolpaths
{
    public interface ILinearToolpath<T> : IToolpath, IEnumerable<T>
    {
        T this[int key] { get; }
    }
}