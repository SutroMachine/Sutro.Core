using g3;
using gs;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Sutro.Core.Support
{
    public class SupportAreaGenerator
    {
        private readonly LayerSupportCalculator layerSupportCalculator;
        private readonly ISupportPointGenerator supportPointGenerator;

        private readonly double printWidth;
        private readonly double mergeDownDilate;
        private readonly double supportGap;
        private readonly double discardHoleSize;
        private readonly double discardHoleArea;
        private readonly double minDiameter;
        private readonly double supportMinDist;
        private readonly bool supportMinZTips;

        private readonly bool enableInterLayerSmoothing = true;

        /// <summary>Initializes a new instance of the SupportAreaGenerator</summary>
        /// <remarks>
        /// The constructor takes in all required parameters; this is cumbersome but forces the calling code to explicitly define the important parameters rather than relying on defaults.
        /// </remarks>
        /// <param name="layerSupportCalculator">Class to calculate which regions of a single layer require support based on the layer below</param>
        /// <param name="supportPointGenerator">Utility class to generate a polygon shape underneath min-z-tips</param>
        /// <param name="mergeDownDilate">Amount to dilate/contract support regions when merging them, to ensure overlap. Using a larger value here has the effect of smoothing out the support polygons. However it can also end up merging disjoint regions...</param>
        /// <param name="printWidth">Characteristic path width; typically the aperture diameter</param>
        /// <param name="supportGap">Space to leave between support polygons and solids</param>
        /// <param name="discardHoleSize">Throw away holes in support regions with bound boxes smaller than this dimension</param>
        /// <param name="discardHoleArea">Throw away holes in support regions with areas smaller than this amount</param>
        /// <param name="minDiameter">Throw away support polygons smaller than this</param>
        /// <param name="supportMinDist">If a support polygon is further than this from model, it will be considered a min-z-tip and get special handling</param>
        /// <param name="supportMinZTips">Whether to add support under min-z-tips</param>
        public SupportAreaGenerator(LayerSupportCalculator layerSupportCalculator,
            ISupportPointGenerator supportPointGenerator,
            double printWidth, double mergeDownDilate, double supportGap,
            double discardHoleSize, double discardHoleArea, double minDiameter, 
            double supportMinDist, bool supportMinZTips)
        {
            this.printWidth = printWidth;
            this.mergeDownDilate = mergeDownDilate;
            this.supportGap = supportGap;
            this.layerSupportCalculator = layerSupportCalculator;
            this.supportPointGenerator = supportPointGenerator;
            this.discardHoleSize = discardHoleSize;
            this.discardHoleArea = discardHoleArea;
            this.minDiameter = minDiameter;
            this.supportMinDist = supportMinDist;
            this.supportMinZTips = supportMinZTips;
        }

        public virtual List<GeneralPolygon2d>[] Compute(PlanarSliceStack slices,
            IReadOnlyList<IReadOnlyCollection<GeneralPolygon2d>> bridgeAreas,
            IReadOnlyList<GeneralPolygon2d> pathClipRegions,
            Action incrementProgress,
            CancellationToken cancellationToken)
        {
            /*
             *  Here is the strategy for computing support areas:
             *    For layer i, support region is union of:
             *         1) Difference[ layer i+1 solids, offset(layer i solids, fOverhangAngleDist) ]
             *              (ie the bit of the next layer we need to support)
             *         2) support region at layer i+1
             *         3) any pre-defined support solids  (ie from user-defined mesh)
             *
             *    Once we have support region at layer i, we subtract expanded layer i solid, to leave a gap
             *         (this is fSupportGapInLayer)
             *
             *    Note that for (2) above, it is frequently the case that support regions in successive
             *    layers are disjoint. To merge these regions, we dilate/union/contract.
             *    The dilation amount is the fMergeDownDilate parameter.
             */

            if (slices.Count <= 1)
                return new List<GeneralPolygon2d>[0];

            /*
             * Step 1: compute absolute support polygon for each layer
             */

            // For layer i, compute support region needed to support layer (i+1)
            // This is the *absolute* support area - no inset for filament width or spacing from model

            var layerSupportAreas = ComputeAbsoluteSupport(slices, bridgeAreas, pathClipRegions, incrementProgress, cancellationToken);

            /*
             * Step 2: sweep support polygons downwards
             */

            // now merge support layers. Process is to track "current" support area,
            // at layer below we union with that layers support, and then subtract
            // that layers solids.
            return Sweep(slices, layerSupportAreas, pathClipRegions, cancellationToken);
        }

        private List<GeneralPolygon2d>[] Sweep(PlanarSliceStack slices, List<GeneralPolygon2d>[] support,
            IReadOnlyList<GeneralPolygon2d> pathClipRegions, CancellationToken cancellationToken)
        {
            var result = new List<GeneralPolygon2d>[slices.Count];

            List<GeneralPolygon2d> prevSupport = support[slices.Count - 1];

            for (int i = slices.Count - 2; i >= 0; --i)
            {
                cancellationToken.ThrowIfCancellationRequested();

                PlanarSlice slice = slices[i];

                // [RMS] smooth the support polygon from the previous layer. if we allow
                // shrinking then they will shrink to nothing, though...need to bound this somehow
                List<GeneralPolygon2d> support_above = new List<GeneralPolygon2d>();
                bool grow = enableInterLayerSmoothing && true;
                bool shrink = enableInterLayerSmoothing && false;

                foreach (GeneralPolygon2d solid in prevSupport)
                {
                    support_above.Add(SmoothSupportPolygon(grow, shrink, solid));
                }

                var combinedSupport = CombineSupport(support[i], support_above);

                // support area we propagate down is combined area minus solid
                prevSupport = ClipperUtil.Difference(combinedSupport, slice.Solids);

                // [TODO] everything after here can be done in parallel in a second pass, right?

                // if we have explicit support, we can union it in now
                if (slice.SupportSolids.Count > 0)
                {
                    combinedSupport = ClipperUtil.Union(combinedSupport, slice.SupportSolids);
                }

                // make sure there is space between solid and support
                List<GeneralPolygon2d> dilatedSolid = ClipperUtil.MiterOffset(slice.Solids, supportGap);
                combinedSupport = ClipperUtil.Difference(combinedSupport, dilatedSolid);

                if (pathClipRegions != null)
                    combinedSupport = ClipperUtil.Intersection(combinedSupport, pathClipRegions, minDiameter * minDiameter);

                result[i] = new List<GeneralPolygon2d>();
                foreach (GeneralPolygon2d poly in combinedSupport)
                {
                    PolySimplification2.Simplify(poly, 0.25 * printWidth);
                    result[i].Add(poly);
                }
            }
            return result;
        }

        private List<GeneralPolygon2d> CombineSupport(List<GeneralPolygon2d> currentLayerSupport, List<GeneralPolygon2d> aboveLayerSupport)
        {
            // union down

            // [TODO] should discard small interior holes here if they don't intersect layer...

            // [RMS] support polygons on successive layers they will not necessarily intersect, because
            // they are offset inwards on each layer. But as we merge down, we want them to be combined.
            // So, we do a dilate / boolean / contract.
            // *But*, doing this can cause undesirable effects on the support polygons in
            // simpler cases, particularly "windy shells" type things. So, if the boolean-of-dilations
            // has the same topology as the input (approximated by count!!), we will just stick
            // with the original polygons
            var combinedSupport = ClipperUtil.Union(aboveLayerSupport, currentLayerSupport);

            if (mergeDownDilate > 0)
            {
                var aboveLayerDilated = ClipperUtil.MiterOffset(aboveLayerSupport, mergeDownDilate);
                var currentLayerDilated = ClipperUtil.MiterOffset(currentLayerSupport, mergeDownDilate);
                var dilatedUnion = ClipperUtil.Union(aboveLayerDilated, currentLayerDilated);
                // [RMS] this is not very sophisticated...
                if (dilatedUnion.Count != combinedSupport.Count)
                {
                    combinedSupport = ClipperUtil.MiterOffset(dilatedUnion, -mergeDownDilate);
                }
            }

            return combinedSupport;
        }

        private GeneralPolygon2d SmoothSupportPolygon(bool grow, bool shrink, GeneralPolygon2d solid)
        {
            GeneralPolygon2d copy = new GeneralPolygon2d();
            copy.Outer = new Polygon2d(solid.Outer);
            if (grow || shrink)
                CurveUtils2.LaplacianSmoothConstrained(copy.Outer, 0.5, 5, mergeDownDilate, shrink, grow);

            // [RMS] here we are also smoothing interior holes. However (in theory) this
            // smoothing might expand the hole outside of the Outer polygon. So if we
            // have holes we intersect with that poly, inset by a printwidth.
            // [TODO] do we really need this? if hole expands, it will still be
            // clipped against model.
            List<GeneralPolygon2d> outer_clip = solid.Holes.Count == 0 ? null : ClipperUtil.MiterOffset(copy, -printWidth);
            foreach (Polygon2d hole in solid.Holes)
            {
                if (hole.Bounds.MaxDim < discardHoleSize || Math.Abs(hole.SignedArea) < discardHoleArea)
                    continue;
                Polygon2d new_hole = new Polygon2d(hole);
                if (grow || shrink)
                    CurveUtils2.LaplacianSmoothConstrained(new_hole, 0.5, 5, mergeDownDilate, shrink, grow);

                List<GeneralPolygon2d> clipped_holes =
                    ClipperUtil.Intersection(new GeneralPolygon2d(new_hole), outer_clip);
                clipped_holes = CurveUtils2.FilterDegenerate(clipped_holes, discardHoleArea);
                foreach (GeneralPolygon2d cliphole in clipped_holes)
                {
                    if (cliphole.Outer.IsClockwise == false)
                        cliphole.Outer.Reverse();
                    copy.AddHole(cliphole.Outer, false);   // ignore any new sub-holes that were created
                }
            }

            return copy;
        }

        private List<GeneralPolygon2d>[] ComputeAbsoluteSupport(PlanarSliceStack slices, IReadOnlyList<IReadOnlyCollection<GeneralPolygon2d>> bridgeAreas, IReadOnlyList<GeneralPolygon2d> pathClipRegions, Action incrementProgress, CancellationToken cancellationToken)
        {
            var result = new List<GeneralPolygon2d>[slices.Count];
#if DEBUG
            Interval1i interval = new Interval1i(0, slices.Count - 1);
            for (int layeri = interval.a; layeri < interval.b; ++layeri)
#else
			gParallel.ForEach(Interval1i.Range(nLayers - 1), (layeri) =>
#endif
            {
                cancellationToken.ThrowIfCancellationRequested();

                var supportPolys = layerSupportCalculator.Calculate(slices[layeri].Solids, slices[layeri + 1].Solids, bridgeAreas[layeri]);

                // now we need to deal with tiny polys. If they are min-z-tips,
                // we want to add larger support regions underneath them.
                // We determine this by measuring distance to this layer.
                // NOTE: we **cannot** discard tiny polys here, because a bunch of
                // tiny per-layer polygons may merge into larger support regions
                // after dilate/contract, eg on angled thin strips.
                supportPolys = HandleTinyPolygons(supportPolys, slices[layeri]);

                // add any explicit support points in this layer as circles
                foreach (Vector2d v in slices[layeri].InputSupportPoints)
                    supportPolys.Add(supportPointGenerator.MakeSupportPointPolygon(v));

                if (pathClipRegions != null)
                    supportPolys = ClipperUtil.Intersection(supportPolys, pathClipRegions);

                result[layeri] = supportPolys;
                incrementProgress?.Invoke();
#if DEBUG
            }
#else
        });
#endif
            result[^1] = new List<GeneralPolygon2d>();
            return result;
        }

        private List<GeneralPolygon2d> HandleTinyPolygons(List<GeneralPolygon2d> supportPolys, PlanarSlice slice)
        {
            List<GeneralPolygon2d> filteredPolys = new List<GeneralPolygon2d>();
            foreach (var poly in supportPolys)
            {
                var bounds = poly.Bounds;
                // big enough to keep
                if (bounds.MaxDim > minDiameter)
                {
                    filteredPolys.Add(poly);
                    continue;
                }

                // Find nearest point. If it is far from print volume, then this is a Min-Z "tip" region.
                // These will get larger polys if SupportMinZTips is enabled
                double dnear_sqr = slice.DistanceSquared(bounds.Center, 2 * supportMinDist);
                if (dnear_sqr > supportMinDist * supportMinDist)
                {
                    if (supportMinZTips)
                        filteredPolys.Add(supportPointGenerator.MakeSupportPointPolygon(bounds.Center));
                    else
                        filteredPolys.Add(supportPointGenerator.MakeSupportPointPolygon(bounds.Center, minDiameter));
                    // else throw it away
                    continue;
                }

                // If we are close the print volume, then maybe we do not need to support this tip.
                // The most conservative test is if this region is supported on two opposite sides.
                // If not, we add a minimal support polygon.
                double d = 1.25 * supportMinDist; double dsqr = d * d;
                Vector2d dx = d * Vector2d.AxisX, dy = d * Vector2d.AxisY;
                int sleft = slice.DistanceSquared(bounds.Center - dx, 2 * d) < dsqr ? 1 : 0;
                int sright = slice.DistanceSquared(bounds.Center + dx, 2 * d) < dsqr ? 1 : 0;
                if (sleft + sright == 2)
                    continue;
                int sfwd = slice.DistanceSquared(bounds.Center + dy, 2 * d) < dsqr ? 1 : 0;
                int sback = slice.DistanceSquared(bounds.Center - dy, 2 * d) < dsqr ? 1 : 0;
                if (sfwd + sback == 2)
                    continue;

                // ok force support
                filteredPolys.Add(supportPointGenerator.MakeSupportPointPolygon(bounds.Center, minDiameter));
            }

            return filteredPolys;
        }
    }
}