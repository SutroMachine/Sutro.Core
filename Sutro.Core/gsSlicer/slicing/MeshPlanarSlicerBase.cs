using g3;
using gs;
using Sutro.Core.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace gs
{
    public abstract class MeshPlanarSlicerBase
    {
        protected readonly IGraphLogger graphLogger;
        protected List<SliceMesh> Meshes = new List<SliceMesh>();

        public Func<bool> CancelF { get; set; } = () => { return false; };
        /// <summary>
        /// How should open paths be handled. Is overridden by
        /// OpenPathsModes for specific meshes
        /// </summary>
        public OpenPathsModes DefaultOpenPathMode { get; set; } = OpenPathsModes.Clipped;

        /// <summary>
        /// If true, then any empty slices at bottom of stack are discarded.
        /// </summary>
        public bool DiscardEmptyBaseSlices { get; set; } = false;

        /// <summary>
        /// provide this function to override default LayerHeighMM
        /// </summary>
        public Func<int, double> LayerHeightF { get; set; } = null;

        /// <summary>
        /// Default Slice height
        /// </summary>
        public double LayerHeightMM { get; set; } = 0.2;

        public int MaxLayerCount { get; set; } = 10000;
        /// <summary>
        /// Often desirable to support a Z-minima tip several layers "up" around it.
        /// This is how many layers.
        /// </summary>
        public int MinZTipExtraLayers { get; set; } = 6;

        /// <summary>
        /// What is the largest floating polygon we will consider a "tip"
        /// </summary>
        public double MinZTipMaxDiam { get; set; } = 2.0;

        /// <summary>
        /// Open-sheet meshes slice into open paths. For OpenPathsModes.Embedded mode, we need
        /// to subtract thickened path from the solids. This is the path thickness.
        /// </summary>
        public double OpenPathDefaultWidthMM { get; set; } = 0.4;

        /// <summary>
        /// Where in layer should we compute slice
        /// </summary>
        public SliceLocations SliceLocation { get; set; } = SliceLocations.MidLine;

        // factory functions you can replace to customize objects/behavior
        public Func<PlanarSliceStack> SliceStackFactoryF { get; set; } = () => { return new PlanarSliceStack(); };

        /// <summary>
        /// Support "tips" (ie z-minima vertices) can be detected geometrically and
        /// added to PlanarSlice.InputSupportPoints.
        /// </summary>
        public bool SupportMinZTips { get; set; } = false;

        // These can be used for progress tracking
        protected int progress = 0;
        public int Progress => progress;
        public int TotalCompute { get; set; } = 0;

        private readonly List<GeneralPolygon2d> validRegions = null;

        /// <summary>
        /// If this is set, all incoming polygons are clipped against it
        /// </summary>
        public ReadOnlyCollection<GeneralPolygon2d> ValidRegions => validRegions?.AsReadOnly() ?? null;

        public bool WasCancelled { get; set; } = false;

        protected MeshPlanarSlicerBase(IGraphLogger graphLogger = null)
        {
            this.graphLogger = graphLogger;
        }

        public bool Add(PrintMeshAssembly assy)
        {
            foreach (var pair in assy.MeshesAndOptions())
                AddMesh(pair.Item1, pair.Item2);
            return true;
        }

        public int AddMesh(DMesh3 mesh, PrintMeshOptions options)
        {
            var sliceMesh = new SliceMesh()
            {
                Mesh = mesh,
                Bounds = mesh.CachedBounds,
                Options = options
            };
            int idx = Meshes.Count;
            Meshes.Add(sliceMesh);
            return idx;
        }

        public int AddMesh(DMesh3 mesh)
        {
            return AddMesh(mesh, PrintMeshOptionsFactory.Default());
        }

        /// <summary>
        /// Slice the meshes and return the slice stack.
        /// </summary>
        public PlanarSliceStack Compute()
        {
            if (Meshes.Count == 0)
                return new PlanarSliceStack();

            var planarSlices = InitializePlanarSlices();

            if (planarSlices.Count > MaxLayerCount)
                throw new ArgumentOutOfRangeException("MaxLayerCount", "MeshPlanarSlicer.Compute: exceeded layer limit. Increase MaxLayerCount.");

            // determine if we have crop objects
            bool cropMeshesExist = false;
            foreach (var mesh in Meshes)
            {
                if (mesh.Options.IsCropRegion)
                    cropMeshesExist = true;
            }

            // assume Resolve() takes 2x as long as meshes...
            TotalCompute = (Meshes.Count * planarSlices.Count) + (2 * planarSlices.Count);
            progress = 0;

            // compute slices separately for each mesh
            foreach (var sliceMesh in Meshes)
            {
                if (Cancelled())
                    break;

                SliceMesh(planarSlices, sliceMesh);
            }

            ResolveSlices(planarSlices, cropMeshesExist);

            // discard spurious empty slices
            return DiscardEmptySlices(planarSlices.Values.ToList(), cropMeshesExist);
        }

        protected virtual void ResolveSlices(ConcurrentDictionary<int, PlanarSlice> planarSlices, bool cropMeshesExist)
        {
            // resolve planar intersections, etc
            Sutro.Core.Utility.Parallel.ForEach(planarSlices.Keys, (i, _) =>
            {
                if (Cancelled())
                    return;

                if (cropMeshesExist && planarSlices[i].InputCropRegions.Count == 0)
                {
                    // don't resolve, we have fully cropped this layer
                }
                else
                {
                    planarSlices[i].Resolve();
                }

                Interlocked.Add(ref progress, 2);
            });
        }

        protected static DGraph2Util.Curves EmptyCurves()
        {
            return new DGraph2Util.Curves()
            {
                Loops = new List<Polygon2d>(),
                Paths = new List<PolyLine2d>()
            };
        }

        protected virtual void AddCavityPolygons(PlanarSlice slice, List<GeneralPolygon2d> polygons, PrintMeshOptions options)
        {
            slice.AddCavityPolygons(polygons);

            if (options.ClearanceXY != 0)
            {
                foreach (var poly in polygons)
                    slice.Cavity_Clearances.Add(poly, options.ClearanceXY);
            }

            if (options.OffsetXY != 0)
            {
                foreach (var poly in polygons)
                    slice.Cavity_Offsets.Add(poly, options.OffsetXY);
            }
        }

        protected virtual void AddCropRegionPolygons(PlanarSlice slice, List<GeneralPolygon2d> polygons, PrintMeshOptions options)
        {
            slice.AddCropRegions(polygons);
        }

        protected void AddCurves(PlanarSlice currentPlaneSlice, ICollection<Polygon2d> polys, ICollection<PolyLine2d> paths,
            PrintMeshOptions meshOptions, bool isClosed)
        {
            OpenPathsModes useOpenMode = GetOpenMeshMode(meshOptions);

            if (isClosed)
            {
                AddCurvesForSolidMesh(currentPlaneSlice, polys, meshOptions);
            }
            else if (useOpenMode != OpenPathsModes.Ignored)
            {
                AddCurvesForOpenMesh(currentPlaneSlice, polys, paths, useOpenMode);
            }
        }

        protected virtual void AddSolidPolygons(PlanarSlice slice, List<GeneralPolygon2d> polygons, PrintMeshOptions options)
        {
            slice.AddPolygons(polygons);

            if (options.ClearanceXY != 0)
            {
                foreach (var poly in polygons)
                    slice.Clearances.Add(poly, options.ClearanceXY);
            }

            if (options.OffsetXY != 0)
            {
                foreach (var poly in polygons)
                    slice.Offsets.Add(poly, options.OffsetXY);
            }
        }

        protected virtual void AddSupportPolygons(PlanarSlice slice, List<GeneralPolygon2d> polygons, PrintMeshOptions options)
        {
            slice.AddSupportPolygons(polygons);
        }

        protected virtual List<GeneralPolygon2d> ApplyValidRegions(List<GeneralPolygon2d> polygonsIn)
        {
            if (ValidRegions == null || ValidRegions.Count == 0)
                return polygonsIn;
            return ClipperUtil.Intersection(polygonsIn, validRegions);
        }

        protected virtual List<PolyLine2d> ApplyValidRegions(List<PolyLine2d> plinesIn)
        {
            if (ValidRegions == null || ValidRegions.Count == 0)
                return plinesIn;
            List<PolyLine2d> clipped = new List<PolyLine2d>();
            foreach (var pline in plinesIn)
                clipped.AddRange(ClipperUtil.ClipAgainstPolygon(validRegions, pline, true));
            return clipped;
        }

        protected virtual bool Cancelled()
        {
            if (WasCancelled)
                return true;
            bool cancel = CancelF();
            if (cancel)
            {
                WasCancelled = true;
                return true;
            }
            return false;
        }

        protected DGraph2Util.Curves ComputePlaneCurves(
            DMesh3 mesh, DMeshAABBTree3 spatial, PlanarSlice slice, bool meshIsSolid)
        {
            var plane = GetSlicePlane(slice);

            var triangles = FindTrianglesIntersectingPlane(mesh, spatial, plane, slice.LayerZSpan.Length);

            if (triangles.Count == 0)
            {
                return EmptyCurves();
            }

            // Compute intersection iso-curves, which produces a 3D graph of undirected edges
            MeshIsoCurves iso = new MeshIsoCurves(mesh, v1 => PlaneF(v1, plane)) { WantGraphEdgeInfo = true };
            iso.Compute(triangles);

            DGraph3 graph = iso.Graph;
            if (graph.EdgeCount == 0)
            {
                return EmptyCurves();
            }

            // If this is a closed solid, any open spurs in the graph are errors
            if (meshIsSolid)
                DGraph3Util.ErodeOpenSpurs(graph);

            // Extract loops and open curves from graph
            var curves3d = DGraph3Util.ExtractCurves(graph, false, iso.ShouldReverseGraphEdge);

            graphLogger?.LogGraph(ConvertGraphTo2d(graph, plane), $"LAYER_SLICE_{slice.Z:F3}");

            return ConvertCurvesToSliceCoordinates(slice, curves3d);
        }

        protected abstract DGraph2Util.Curves ConvertCurvesToSliceCoordinates(PlanarSlice slice, DGraph3Util.Curves curves3d);

        protected DGraph2 ConvertGraphTo2d(DGraph3 graph, Plane3d plane)
        {
            DGraph2 graph2 = new DGraph2();
            Dictionary<int, int> mapV = new Dictionary<int, int>();
            foreach (int vid in graph.VertexIndices())
                mapV[vid] = graph2.AppendVertex(graph.GetVertex(vid).xy);
            foreach (int eid in graph.EdgeIndices())
                graph2.AppendEdge(mapV[graph.GetEdge(eid).a], mapV[graph.GetEdge(eid).b]);
            return graph2;
        }

        protected PlanarSliceStack DiscardEmptySlices(IList<PlanarSlice> slices, bool hasCropObjects)
        {
            int last = slices.Count - 1;
            while (slices[last].IsEmpty && last > 0)
                last--;
            int first = 0;
            if (DiscardEmptyBaseSlices || hasCropObjects)
            {
                while (slices[first].IsEmpty && first < slices.Count)
                    first++;
            }

            PlanarSliceStack stack = SliceStackFactoryF();
            for (int k = first; k <= last; ++k)
                stack.Add(slices[k]);

            if (SupportMinZTips)
                stack.AddMinZTipSupportPoints(MinZTipMaxDiam, MinZTipExtraLayers);
            return stack;
        }

        protected abstract List<int> FindTrianglesIntersectingPlane(DMesh3 mesh, DMeshAABBTree3 spatial, Plane3d plane, double thickness);

        protected virtual double GetLayerHeight(int layer_i)
        {
            return (LayerHeightF != null) ? LayerHeightF(layer_i) : LayerHeightMM;
        }

        protected abstract Plane3d GetSlicePlane(PlanarSlice slice);

        protected abstract ConcurrentDictionary<int, PlanarSlice> InitializePlanarSlices();

        protected abstract DGraph2Util.Curves JitterZ(PlanarSlice planarSlice, SliceMesh sliceMesh, DMeshAABBTree3 spatial, bool meshIsClosed);

        protected bool NoIntersectionsFound(bool meshIsClosed, DGraph2Util.Curves curves)
        {
            return (meshIsClosed && curves.Loops.Count == 0) || (meshIsClosed == false && curves.Loops.Count == 0 && curves.Paths.Count == 0);
        }

        protected abstract double PlaneF(Vector3d v, Plane3d plane);

        protected virtual void SliceMesh(ConcurrentDictionary<int, PlanarSlice> planarSlices, SliceMesh sliceMesh)
        {
            PrintMeshOptions mesh_options = sliceMesh.Options;

            // [TODO] should we hang on to this spatial? or should it be part of assembly?
            DMeshAABBTree3 spatial = new DMeshAABBTree3(sliceMesh.Mesh, true);
            AxisAlignedBox3d bounds = sliceMesh.Bounds;

            bool isOpenMesh = (mesh_options.IsOpen) ? false : sliceMesh.Mesh.IsClosed();

            // each layer is independent so we can do in parallel
            Sutro.Core.Utility.Parallel.ForEach(planarSlices.Keys, (i, _) =>
            {
                if (Cancelled())
                    return;

                // compute cut
                var curves = ComputePlaneCurves(sliceMesh.Mesh, spatial, planarSlices[i], isOpenMesh);

                // If we didn't hit anything, try again with jittered plane
                if (NoIntersectionsFound(isOpenMesh, curves))
                {
                    curves = JitterZ(planarSlices[i], sliceMesh, spatial, isOpenMesh);
                }

                AddCurves(planarSlices[i], curves.Loops, curves.Paths, mesh_options, isOpenMesh);
                Interlocked.Increment(ref progress);
            });
        }

        private void AddCurvesForOpenMesh(PlanarSlice currentPlaneSlice, ICollection<Polygon2d> polys, ICollection<PolyLine2d> paths, OpenPathsModes useOpenMode)
        {
            // [TODO]
            //   - does not really handle clipped polygons properly, there will be an extra break somewhere...
            List<PolyLine2d> all_paths = new List<PolyLine2d>(paths);
            foreach (Polygon2d poly in polys)
                all_paths.Add(new PolyLine2d(poly, true));

            List<PolyLine2d> open_polylines = ApplyValidRegions(all_paths);
            foreach (PolyLine2d pline in open_polylines)
            {
                if (useOpenMode == OpenPathsModes.Embedded)
                    currentPlaneSlice.AddEmbeddedPath(pline);
                else
                    currentPlaneSlice.AddClippedPath(pline);
            }
        }

        private void AddCurvesForSolidMesh(PlanarSlice currentPlaneSlice, ICollection<Polygon2d> polys, PrintMeshOptions meshOptions)
        {
            // construct planar complex and "solids"
            // (ie outer polys and nested holes)
            var complex = new PlanarComplex();
            foreach (var poly in polys)
                complex.Add(poly);

            var options = PlanarComplex.FindSolidsOptions.Default;
            options.WantCurveSolids = false;
            options.SimplifyDeviationTolerance = 0.001;
            options.TrustOrientations = true;
            options.AllowOverlappingHoles = true;

            PlanarComplex.SolidRegionInfo solids = complex.FindSolidRegions(options);
            List<GeneralPolygon2d> solid_polygons = ApplyValidRegions(solids.Polygons);

            if (meshOptions.IsSupport)
                AddSupportPolygons(currentPlaneSlice, solid_polygons, meshOptions);
            else if (meshOptions.IsCavity)
                AddCavityPolygons(currentPlaneSlice, solid_polygons, meshOptions);
            else if (meshOptions.IsCropRegion)
                AddCropRegionPolygons(currentPlaneSlice, solid_polygons, meshOptions);
            else
                AddSolidPolygons(currentPlaneSlice, solid_polygons, meshOptions);
        }

        private OpenPathsModes GetOpenMeshMode(PrintMeshOptions meshOptions)
        {
            return (meshOptions.OpenPathMode == OpenPathsModes.Default) ?
                DefaultOpenPathMode : meshOptions.OpenPathMode;
        }
    }
}