using System;

namespace Sutro.Core.Toolpaths
{
    /// <summary>
    /// PathVertex.Flags field is 3 ints that can be used for whatever purpose.
    /// First int we assume is one of these values, or a client-defined value.
    /// </summary>
    [Flags]
    public enum TPVertexFlags
    {
        None = 0,
        IsConnector = 1,            // connects spans of a linear fill. also currently not used (!)
        IsSupport = 1 << 1,        // unused currently?
        IsPathStart = 1 << 2,
        IsPathEnd = 1 << 3,
        IsWipeStart = 1 << 4,
        IsWipeEnd = 1 << 5,
        IsPrime = 1 << 6
    }
}