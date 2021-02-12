using System.Collections.Generic;

namespace gs
{
    public interface IToolpathSet : IToolpath, IEnumerable<IToolpath>
    {
    }
}