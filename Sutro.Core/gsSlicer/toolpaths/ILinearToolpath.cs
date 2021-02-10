using System.Collections.Generic;

namespace gs
{
    public interface ILinearToolpath<T> : IToolpath, IEnumerable<T>
    {
        T this[int key] { get; }
    }
}