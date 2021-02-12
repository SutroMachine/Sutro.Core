using g3;
using System.Collections.Generic;

namespace Sutro.Core.Toolpaths
{
    public interface IToolpath
    {
        ToolpathTypes Type { get; }
        bool IsPlanar { get; }
        bool IsLinear { get; }

        Vector3d StartPosition { get; }
        Vector3d EndPosition { get; }
        AxisAlignedBox3d Bounds { get; }

        bool HasFinitePositions { get; }

        IEnumerable<Vector3d> AllPositionsItr();
    }
}