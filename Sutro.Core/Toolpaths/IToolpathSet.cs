using System.Collections.Generic;

namespace Sutro.Core.Toolpaths
{
    public interface IToolpathSet : IToolpath, IEnumerable<IToolpath>
    {
    }
}