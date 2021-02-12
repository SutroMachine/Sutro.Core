using g3;
using System.Collections.Generic;

namespace gs
{
    public class PlaneIntersectionTraversal : DMeshAABBTree3.TreeTraversal
    {
        public DMesh3 Mesh { get; }
        public List<int> Triangles { get; } = new List<int>();
        public double PlaneZHeight { get; }

        public PlaneIntersectionTraversal(DMesh3 mesh, double z)
        {
            Mesh = mesh;
            PlaneZHeight = z;

            NextBoxF = (box, depth) =>
            {
                return (PlaneZHeight >= box.Min.z && PlaneZHeight <= box.Max.z);
            };

            NextTriangleF = (triangleIndex) =>
            {
                AxisAlignedBox3d box = Mesh.GetTriBounds(triangleIndex);
                if (PlaneZHeight >= box.Min.z && z <= box.Max.z)
                    Triangles.Add(triangleIndex);
            };
        }
    }
}