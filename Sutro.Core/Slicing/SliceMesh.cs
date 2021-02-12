using g3;
using Sutro.Core.Models;

namespace Sutro.Core.Slicing
{
    public class SliceMesh
    {
        public DMesh3 Mesh { get; set; }
        public AxisAlignedBox3d Bounds { get; set; }
        public PrintMeshOptions Options { get; set; }
    }
}