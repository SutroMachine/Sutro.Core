using g3;
using gs.FillTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace gs
{
    /// <summary>
    /// Fill the input GPolygon with N offset shells. Generates list of 2d paths (.Shells) and
    /// also returns border polygons of "inner" region inside shells (.InnerPolygons)
    ///
    /// Parameters:
    ///     .Layers:       number of shells
    ///     .ToolWidth:    thickness of 'tool'
	///     .PathSpacing:  spacing between paths. Generally should be >= ToolWidth
    ///
    /// Options:
    ///     .InsetFromInputPolygon    : if true, initial shell is inset half a ToolWidth from input Polygon
    ///                                 (ie outer edge of tool would be "on" Polygon)
    ///     .InsetInnerPolygons       : if true, inner polygons are inset a tool-width from innermost shell (ie are
    ///                                 path you would put next shell on). If false, inner polygons lie on innermost shell.
    ///     .FilterSelfOverlaps       : if true, we try to remove areas of path that would self-overlap (default false)
    ///     .PreserveOuterShells      : if true, we don't apply this removal to outermost shells (default true)
    ///     .SelfOverlapTolerance     : distance that counts as self-overlap (default 0.3)
    /// </summary>
	public class ShellsFillPolygon : IShellsFillPolygon
    {
        // polygon to fill
        public GeneralPolygon2d Polygon { get; set; }

        // parameters
        public int Layers { get; set; } = 2;

        public double ToolWidth { get; set; } = 0.4;
        public double PathSpacing { get; set; } = 0.4;

        public double DiscardTinyPerimeterLengthMM { get; set; } = 1.0;
        public double DiscardTinyPolygonAreaMM2 { get; set; } = 1.0;

        public bool EnableThinCheck { get; set; } = false;

        // Replace this to use a different inset distance calculations
        public Func<int, double> InsetDistanceFactoryF { get; set; }

        // When offsets collide, we try to find polyline paths that will "fit"
        // This is multiplier on ToolWidth, we discard path segments within
        // ToolWidth*Multiplier distance from previous shell
        public double ToolWidthClipMultiplier { get; set; } = 0.8;

        // if true, we inset half of tool-width from Polygon,
        // otherwise first layer is polygon
        public double InsetFromInputPolygonX { get; set; } = 0.5;

        // if InsetFromInputPolygonX > 0, and this is true, we will do whatever
        // we can to make sure that the inset does not have large shape/topology
        // deviations, going as far as ignore the inset if necessary
        public bool PreserveInputInsetTopology { get; set; } = false;

        // if true, inset InnerPolygons by a tool-width from last Shell,
        // otherwise InnerPolygons lies on that Shell
        public bool InsetInnerPolygons { get; set; } = true;

        // if true, we try to filter out self-overlaps (is expensive)
        public bool FilterSelfOverlaps { get; set; } = false;

        public ReadOnlyCollection<GeneralPolygon2d> InitialInsetPolygons { get; protected set; }

        public bool PreserveOuterShells { get; set; } = true;         // if true, we do not try to filter self-overlaps for shell 0
        public double SelfOverlapTolerance { get; set; } = 0.3;

        public bool OuterShellLast { get; set; } = false;             // if true, outer shell is scheduled last

        protected readonly IFillType fillType;
        protected readonly IFillType firstShellFillType;

        // Outputs

        /// <summary>
        /// Shell layers are sorted outer to inner. Size of list == layers
        /// </summary>
        public List<FillCurveSet2d> Shells { get; set; }

        /// <summary>
        /// Return shell paths groups. Same as .Shells property, but
        /// here returned nesting order is [1...N,0]
        /// </summary>
        public List<FillCurveSet2d> GetFillCurves()
        {
            if (OuterShellLast)
            {
                List<FillCurveSet2d> result = new List<FillCurveSet2d>();
                for (int k = 1; k < Shells.Count; ++k)
                    result.Add(Shells[k]);
                if (Shells.Count > 0)
                    result.Add(Shells[0]);
                return result;
            }
            else
                return Shells;
        }

        // remaining interior polygons (to fill w/ other strategy, etc)
        public List<GeneralPolygon2d> InnerPolygons { get; set; }
        public List<GeneralPolygon2d> GetInnerPolygons()
        {
            return InnerPolygons;
        }

        public ShellsFillPolygon(GeneralPolygon2d poly, IFillType fillType, IFillType firstShellFillType = null)
        {
            Polygon = poly;
            Shells = new List<FillCurveSet2d>();

            InsetDistanceFactoryF = (shell_i) =>
            {
                if (shell_i == 0)
                    return ToolWidth * InsetFromInputPolygonX;
                else
                    return PathSpacing;
            };

            this.fillType = fillType;
            this.firstShellFillType = firstShellFillType;
        }

        public virtual bool Compute()
        {
            double thin_check_offset = ToolWidth * 0.45;
            double thin_check_thresh_sqr = Math.Pow(ToolWidth * 0.3, 2);

            // First shell is either polygon, or inset from that polygon
            var current = (InsetFromInputPolygonX != 0)
                ? ComputeInitialInsetPolygon(PreserveInputInsetTopology)
                : new List<GeneralPolygon2d>() { Polygon };

            // Save the initial inset
            InitialInsetPolygons = current.AsReadOnly();

            var current_prev = new List<GeneralPolygon2d>();

            if (current.Count == 0)
            {
                HandleTinyPolygon();
                return true;
            }

            // convert previous layer to shell, and then compute next layer
            List<GeneralPolygon2d> failedShells = new List<GeneralPolygon2d>();
            List<GeneralPolygon2d> nextShellTooThin = new List<GeneralPolygon2d>();
            for (int i = 0; i < Layers; ++i)
            {
                FillCurveSet2d paths = ShellPolysToPaths(current, i);
                Shells.Add(paths);
                double insetDistance = InsetDistanceFactoryF(i + 1);

                List<GeneralPolygon2d> all_next = new List<GeneralPolygon2d>();
                foreach (GeneralPolygon2d gpoly in current)
                {
                    var offsets = InsetGPoly(gpoly, insetDistance);

                    List<GeneralPolygon2d> filtered = new List<GeneralPolygon2d>();
                    foreach (var v in offsets)
                    {
                        bool bTooSmall = (v.Perimeter < DiscardTinyPerimeterLengthMM ||
                                          v.Area < DiscardTinyPolygonAreaMM2);
                        if (bTooSmall)
                            continue;

                        if (EnableThinCheck && is_too_thin(v, thin_check_offset, thin_check_thresh_sqr))
                            nextShellTooThin.Add(v);
                        else
                            filtered.Add(v);
                    }

                    if (filtered.Count == 0)
                        failedShells.Add(gpoly);
                    else
                        all_next.AddRange(filtered);
                }

                ObserveInset(current, all_next, insetDistance);

                current_prev = current;
                current = all_next;
            }

            // remaining inner polygons
            if (InsetInnerPolygons)
            {
                InnerPolygons = current;
                InnerPolygons.AddRange(nextShellTooThin);
            }
            else
            {
                InnerPolygons = new List<GeneralPolygon2d>();
                foreach (var gpoly in current_prev)
                {
                    InnerPolygons.AddRange(InsetGPoly(gpoly, 0));
                }
                InnerPolygons.AddRange(nextShellTooThin);
            }
            return true;
        }

        protected virtual void ObserveInset(List<GeneralPolygon2d> current, List<GeneralPolygon2d> inset, double spacing)
        {
        }

        protected virtual List<GeneralPolygon2d> InsetGPoly(GeneralPolygon2d gpoly, double insetDistance)
        {
            if (insetDistance == 0)
                return new List<GeneralPolygon2d>() { gpoly };

            return ClipperUtil.ComputeOffsetPolygon(gpoly, -insetDistance, true);
        }

        /// <summary>
        /// Compute inset from input polygon. If requested, will check for
        /// topological changes and try smaller insets, and if that fails,
        /// will just return input polygon.
        /// </summary>
        protected virtual List<GeneralPolygon2d> ComputeInitialInsetPolygon(
            bool bForcePreserveTopology)
        {
            double fInset = InsetDistanceFactoryF(0);
            List<GeneralPolygon2d> insetPolys =
                ClipperUtil.MiterOffset(Polygon, -fInset);

            if (!bForcePreserveTopology)
                return insetPolys;

            if (check_large_topology_change(Polygon, insetPolys, fInset))
            {
                fInset /= 2;
                insetPolys = ClipperUtil.MiterOffset(Polygon, -fInset);
                if (check_large_topology_change(Polygon, insetPolys, fInset))
                    insetPolys = new List<GeneralPolygon2d>() { Polygon };
            }

            return insetPolys;
        }

        /// <summary>
        /// Check for significant topology/area changes in polygon inset.
        /// Currently returns true if:
        ///   1) inset has diff # of polygons or holes in single poly
        ///   2) area change is significantly larger than what we expect
        ///      (inset-band area estimated as perimeter_len * k * inset_dist)
        /// </summary>
        protected virtual bool check_large_topology_change(
            GeneralPolygon2d input, List<GeneralPolygon2d> inset, double fInsetDist)
        {
            if (inset.Count != 1 || inset[0].Holes.Count != input.Holes.Count)
                return true;

            double orig_area = Math.Abs(input.Area);
            double perim = input.Perimeter;
            double perim_band_area = perim * fInsetDist;
            double est_inset_area = orig_area - 1.5 * perim_band_area;

            double inset_area = 0;
            foreach (var poly in inset)
                inset_area += Math.Abs(poly.Area);

            if (inset_area < est_inset_area)
                return true;

            return false;
        }

        /// <summary>
        /// Fallback to deal with very tiny polygons that disappear when insetting.
        /// This happens at Z-minima-tips, which can be a problem because it may leave
        /// gaps between layers. For tips we draw a tiny circle.
        /// For elongated shapes we...?? currently do something dumb.
        /// Probably should use robust thinning!
        /// </summary>
        public virtual void HandleTinyPolygon()
        {
            //(InsetFromInputPolygon) ?
            //ClipperUtil.ComputeOffsetPolygon(Polygon, -ToolWidth / 2, true) :
            AxisAlignedBox2d bounds = Polygon.Bounds;
            if (bounds.MaxDim < ToolWidth)
            {
                GeneralPolygon2d min_poly = new GeneralPolygon2d(Polygon2d.MakeCircle(ToolWidth / 4, 6));
                min_poly.Outer.Translate(bounds.Center);
                FillCurveSet2d paths = ShellPolysToPaths(new List<GeneralPolygon2d>() { min_poly }, 0);
                Shells.Add(paths);
            }
            else
            {
                FillCurveSet2d paths = ShellPolysToPaths(new List<GeneralPolygon2d>() { Polygon }, 0);
                Shells.Add(paths);
            }

            InnerPolygons = new List<GeneralPolygon2d>();
        }

        /// <summary>
        /// Convert the input polygons to a set of paths.
        /// If FilterSelfOverlaps=true, then the paths will be clipped against
        /// themselves, in an attempt to avoid over-printing.
        /// </summary>
        public virtual FillCurveSet2d ShellPolysToPaths(List<GeneralPolygon2d> shell_polys, int nShell)
        {
            FillCurveSet2d paths = new FillCurveSet2d();

            IFillType currentFillType = nShell == 0 ? firstShellFillType ?? fillType : fillType;

            if (!FilterSelfOverlaps)
            {
                foreach (var shell in shell_polys)
                {
                    paths.Append(new FillLoop<FillSegment>(shell.Outer.Vertices) { FillType = currentFillType, PerimeterOrder = nShell });
                    foreach (var hole in shell.Holes)
                    {
                        paths.Append(new FillLoop<FillSegment>(hole.Vertices) { FillType = currentFillType, PerimeterOrder = nShell, IsHoleShell = true });
                    }
                }
                return paths;
            }

            int outer_shell_edgegroup = 100;
            foreach (GeneralPolygon2d shell in shell_polys)
            {
                var repair = new PathOverlapRepair
                {
                    OverlapRadius = SelfOverlapTolerance
                };
                repair.Add(shell, outer_shell_edgegroup);

                // Ideally want to presreve outermost shell of external perimeters.
                // However in many cases internal holes are 'too close' to outer border.
                // So we will still apply to those, but use edge filter to preserve outermost loop.
                // [TODO] could we be smarter about this somehow?
                if (PreserveOuterShells && nShell == 0)
                    repair.PreserveEdgeFilterF = (eid) => { return repair.Graph.GetEdgeGroup(eid) == outer_shell_edgegroup; };

                repair.Compute();

                var c = DGraph2Util.ExtractCurves(repair.GetResultGraph());

                #region Borrow nesting calculations from PlanarSlice to enforce winding direction

                PlanarComplex complex = new PlanarComplex();
                foreach (Polygon2d poly in c.Loops)
                    complex.Add(poly);

                PlanarComplex.FindSolidsOptions options
                             = PlanarComplex.FindSolidsOptions.Default;
                options.WantCurveSolids = false;
                options.SimplifyDeviationTolerance = 0.001;
                options.TrustOrientations = false;
                options.AllowOverlappingHoles = false;

                PlanarComplex.SolidRegionInfo solids = complex.FindSolidRegions(options);
                foreach (var polygon in solids.Polygons)
                {
                    polygon.EnforceCounterClockwise();
                    paths.Append(new FillLoop<FillSegment>(polygon.Outer.Vertices) { 
                        FillType = currentFillType, 
                        PerimeterOrder = nShell });
                    foreach (var hole in polygon.Holes)
                    {
                        paths.Append(new FillLoop<FillSegment>(hole.Vertices)
                        {
                            FillType = currentFillType,
                            PerimeterOrder = nShell,
                            IsHoleShell = true,
                        });
                    }
                }

                #endregion Borrow nesting calculations from PlanarSlice to enforce winding direction

                foreach (var polyline in c.Paths)
                {
                    if (polyline.ArcLength < DiscardTinyPerimeterLengthMM)
                        continue;
                    if (polyline.Bounds.MaxDim < DiscardTinyPerimeterLengthMM)
                        continue;
                    paths.Append(new FillCurve<FillSegment>(polyline) { FillType = currentFillType, PerimeterOrder = nShell });
                }
            }
            return paths;
        }

        protected virtual List<FillBase> thin_offset(GeneralPolygon2d p)
        {
            var result = new List<FillBase>();

            // to support non-hole thin offsets we need to return polylines
            if (p.Holes.Count == 0)
                return result;

            // compute desired offset from outer polygon
            GeneralPolygon2d outer = new GeneralPolygon2d(p.Outer);
            List<GeneralPolygon2d> offsets =
                ClipperUtil.ComputeOffsetPolygon(outer, -ToolWidth, true);
            if (offsets == null || offsets.Count == 0)
                return result;

            double clip_dist = ToolWidth * ToolWidthClipMultiplier;
            foreach (GeneralPolygon2d offset_poly in offsets)
            {
                var clipped = clip_to_band(offset_poly.Outer, p, clip_dist);
                result.AddRange(clipped);
            }

            return result;
        }

        protected virtual Polygon2d iterative_offset(GeneralPolygon2d poly, double fDist, int nSteps)
        {
            int N = poly.Outer.VertexCount;
            double max_step = fDist / nSteps;

            Polygon2d cur = new Polygon2d(poly.Outer);
            for (int i = 0; i < N; ++i)
            {
                Vector2d n = cur.GetTangent(i).Perp;
                cur[i] = cur[i] + max_step * n;
            }

            return cur;
        }

        // (approximately) clip insetPoly to band around clipPoly.
        // vertices are discarded if outside clipPoly, or within clip_dist
        // remaining polylines are returned
        // In all-pass case currently returns polyline w/ explicit first==last vertices
        protected virtual List<FillBase> clip_to_band(Polygon2d insetpoly, GeneralPolygon2d clipPoly, double clip_dist)
        {
            double clipSqr = clip_dist * clip_dist;

            int N = insetpoly.VertexCount;
            Vector2d[] midline = new Vector2d[N];
            bool[] clipped = new bool[N];
            int nClipped = 0;
            for (int i = 0; i < N; ++i)
            {
                Vector2d po = insetpoly[i];
                if (!clipPoly.Contains(po))
                {
                    clipped[i] = true;
                    nClipped++;
                    continue;
                }

                double distSqr = clipPoly.DistanceSquared(po, out _, out _, out _);
                if (distSqr < clipSqr)
                {
                    clipped[i] = true;
                    nClipped++;
                    continue;
                }

                // not ideal...
                midline[i] = po;
            }
            if (nClipped == N)
                return new List<FillBase>();
            if (nClipped == 0)
            {
                var all = new FillCurve<FillSegment>(midline);
                return new List<FillBase>() { all.CloseCurve() };
            }

            return find_polygon_spans(midline, clipped);
        }

        // extract set of spans from poly where clipped=false
        protected virtual List<FillBase> find_polygon_spans(Vector2d[] poly, bool[] clipped)
        {
            // assumption: at least one vtx is clipped
            int iStart = 0;

            // handle no-wrap case
            if (!clipped[iStart] && clipped[poly.Length - 1])
            {
                iStart = 0;
            }
            else
            {
                while (clipped[iStart])     // find first non-clipped pt
                    iStart++;
            }

            var result = new List<FillBase>();
            int iCur = iStart;
            bool done = false;

            while (!done)
            {
                var vertices = new List<Vector2d>();
                do
                {
                    vertices.Add(poly[iCur]);
                    iCur = (iCur + 1) % poly.Length;
                } while (!clipped[iCur] && iCur != iStart);

                if (vertices.Count > 1)
                    result.Add(new FillCurve<FillSegment>(vertices));

                while (clipped[iCur] && iCur != iStart)
                    iCur++;

                if (iCur == iStart)
                    done = true;
            }

            return result;
        }

        // approximately check thickness of poly. For each segment, offset by check_offset*seg_normal,
        // then find distance to nearset point on poly. If distance_sqr is < mindist_sqr,
        // then we are below thin-tolerance.
        //
        // Currently returns true/false test, which is stupid...
        //
        // Will definitely fail on: squares (w/ short seg near edge), thin narrow bits... ??
        protected virtual bool is_too_thin(GeneralPolygon2d poly, double check_offset, double mindist_sqr)
        {
            Debug.Assert(mindist_sqr < 0.95 * check_offset * check_offset);

            bool failed = false;
            Action<Segment2d> seg_checkF = (seg) =>
            {
                if (failed)
                    return;
                if (seg.Length < 0.01)  // not robust if segment is too short
                    return;
                Vector2d n = -seg.Direction.Perp;
                Vector2d pt = seg.Center + check_offset * n;
                int iHole, iSeg; double segT;
                double dist_sqr = poly.DistanceSquared(pt, out iHole, out iSeg, out segT);
                if (dist_sqr < mindist_sqr)
                    failed = true;
            };
            gParallel.ForEach(poly.AllSegmentsItr(), seg_checkF);
            return failed;
        }
    }
}