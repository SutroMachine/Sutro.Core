using g3;
using System.Collections.Generic;
using System.Linq;

namespace Sutro.Core.Toolpaths
{
    // Just a utility class we can subclass to create custom "marker" paths
    // in the path stream.
    public class SentinelToolpath : IToolpath
    {
        public virtual ToolpathTypes Type
        {
            get
            {
                return ToolpathTypes.Custom;
            }
        }

        public virtual bool IsPlanar
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsLinear
        {
            get
            {
                return false;
            }
        }

        public virtual Vector3d StartPosition
        {
            get
            {
                return Vector3d.Zero;
            }
        }

        public virtual Vector3d EndPosition
        {
            get
            {
                return Vector3d.Zero;
            }
        }

        public virtual AxisAlignedBox3d Bounds
        {
            get
            {
                return AxisAlignedBox3d.Zero;
            }
        }

        public bool HasFinitePositions
        {
            get { return false; }
        }

        public IEnumerable<Vector3d> AllPositionsItr()
        {
            return Enumerable.Empty<Vector3d>();
        }
    }
}