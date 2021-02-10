using g3;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace gs
{
    [Obsolete("Use the MeshSlicerHorizontalPlanes class intead")]
    public class MeshPlanarSlicer : MeshSlicerHorizontalPlanes
    {
    }

    /// <summary>
    /// Computes a PlanarSliceStack from a set of input meshes, by horizonally
    /// slicing them at regular Z-intervals. This is where we need to sort out
    /// any complications like overlapping shells, etc. Much of that work is
    /// done in PlanarSlice.resolve().
    ///
    /// The input meshes are not modified in this process.
    /// </summary>
	public class MeshSlicerHorizontalPlanes : MeshPlanarSlicerBase
    {
        /// <summary>
        /// Normally we slice in interval [zmin,zmax]. Set this to 0 if you
        /// want to slice [0,zmax].
        /// </summary>
        public double SetMinZValue { get; set; } = double.MinValue;

        public Func<Interval1d, double, int, PlanarSlice> SliceFactoryF { get; set; } =
            (ZSpan, ZHeight, layerIndex) => new PlanarSlice()
            {
                LayerZSpan = ZSpan,
                LayerIndex = layerIndex,
                Z = ZHeight,
            };

        public MeshSlicerHorizontalPlanes(IGraphLogger graphLogger = null) : base(graphLogger)
        {
        }

        protected override DGraph2Util.Curves ConvertCurvesToSliceCoordinates(PlanarSlice slice, DGraph3Util.Curves curves3d)
        {
            var curves2d = EmptyCurves();
            curves2d.Loops = new List<Polygon2d>(curves3d.Loops.Count);
            for (int li = 0; li < curves3d.Loops.Count; ++li)
            {
                DCurve3 loop = curves3d.Loops[li];
                curves2d.Loops.Add(new Polygon2d());
                foreach (Vector3d v in loop.Vertices)
                    curves2d.Loops[li].AppendVertex(v.xy);
            }

            curves2d.Paths = new List<PolyLine2d>(curves3d.Paths.Count);
            for (int pi = 0; pi < curves3d.Paths.Count; ++pi)
            {
                DCurve3 span = curves3d.Paths[pi];
                curves2d.Paths.Add(new PolyLine2d());
                foreach (Vector3d v in span.Vertices)
                    curves2d.Paths[pi].AppendVertex(v.xy);
            }

            return curves2d;
        }

        protected override List<int> FindTrianglesIntersectingPlane(DMesh3 mesh, DMeshAABBTree3 spatial, Plane3d plane, double length)
        {
            // Find list of triangles that intersect this slice
            PlaneIntersectionTraversal planeIntr = new PlaneIntersectionTraversal(mesh, plane.Constant);
            spatial.DoTraversal(planeIntr);
            return planeIntr.Triangles;
        }

        protected override Plane3d GetSlicePlane(PlanarSlice slice)
        {
            return new Plane3d(Vector3d.AxisZ, slice.Z);
        }

        protected override ConcurrentDictionary<int, PlanarSlice> InitializePlanarSlices()
        {
            // construct layers
            var planarSlices = new ConcurrentDictionary<int, PlanarSlice>();

            Interval1d zrange = Interval1d.Empty;
            foreach (var meshinfo in Meshes)
            {
                zrange.Contain(meshinfo.Bounds.Min.z);
                zrange.Contain(meshinfo.Bounds.Max.z);
            }
            if (SetMinZValue != double.MinValue)
                zrange.a = SetMinZValue;

            double cur_layer_z = zrange.a;
            int layer_i = 0;
            while (cur_layer_z < zrange.b)
            {
                double layer_height = GetLayerHeight(layer_i);
                double z = cur_layer_z;
                Interval1d zspan = new Interval1d(z, z + layer_height);
                if (SliceLocation == SliceLocations.EpsilonBase)
                    z += 0.01 * layer_height;
                else if (SliceLocation == SliceLocations.MidLine)
                    z += 0.5 * layer_height;

                PlanarSlice slice = SliceFactoryF(zspan, z, layer_i);
                slice.EmbeddedPathWidth = OpenPathDefaultWidthMM;
                planarSlices.TryAdd(layer_i, slice);

                layer_i++;
                cur_layer_z += layer_height;
            }

            return planarSlices;
        }

        protected override DGraph2Util.Curves JitterZ(PlanarSlice planarSlice, SliceMesh sliceMesh, DMeshAABBTree3 spatial, bool meshIsClosed)
        {
            double jitterz = planarSlice.LayerZSpan.Interpolate(0.75);
            planarSlice.Z = jitterz;
            return ComputePlaneCurves(sliceMesh.Mesh, spatial, planarSlice, meshIsClosed);
        }

        protected override double PlaneF(Vector3d vertex, Plane3d plane)
        {
            return vertex.z - plane.Constant;
        }
    }
}