using g3;
using gs;
using Sutro.Core.Parallel;
using System.Collections.Generic;
using System.Threading;

namespace Sutro.Core.Bridging
{
    public class BridgeRegionCalculator : IBridgeRegionCalculator
    {
        private readonly IParallelActor _parallelActor;

        private readonly double _maxBridgeDistance;
        private readonly double _bridgeToleranceSquared;
        private readonly double _extraExpansion;
        private readonly double _minArea;

        public BridgeRegionCalculator(IParallelActor parallelActor, double maxBridgeDistance,
            double bridgeTolerance, double minArea, double extraExpansion)
        {
            _parallelActor = parallelActor;
            _maxBridgeDistance = maxBridgeDistance;
            _bridgeToleranceSquared = bridgeTolerance * bridgeTolerance;
            _minArea = minArea;
            _extraExpansion = extraExpansion;
        }

        public virtual List<GeneralPolygon2d>[] CalculateBridgeRegions(PlanarSliceStack slices,
            CancellationToken cancellationToken)
        {
            var bridgeAreas = new List<GeneralPolygon2d>[slices.Count];

            if (slices.Count <= 1)
                return bridgeAreas;

            _parallelActor.ForEach(Interval1i.Range(slices.Count - 1),
                new System.Threading.Tasks.ParallelOptions() { CancellationToken = cancellationToken },
                (layerIndex) =>
                {
                    bridgeAreas[layerIndex] = CalculateLayer(slices[layerIndex], slices[layerIndex + 1]);
                });

            bridgeAreas[^1] = new List<GeneralPolygon2d>();

            return bridgeAreas;
        }

        public virtual List<GeneralPolygon2d> CalculateLayer(PlanarSlice currentSlice, PlanarSlice nextSlice)
        {
            // To find bridgeable regions, we compute all floating regions in next layer.
            // Then we look for polys that are bridgeable, ie thing enough and fully anchored.

            // [RMS] bridge area is (next_solids - solids). However, for meshes with slight variations
            // in identical stacked polygons (eg like created from mesh extrusions), there will be thousands
            // of tiny polygons. We can filter them, but just computing them can take an enormous amount of time.
            // So, we slightly offset the slice here. This means the bridge poly will be slightly under-sized,
            // the assumption is we will be adding extra overlap anyway
            List<GeneralPolygon2d> expandPolys = ClipperUtil.MiterOffset(currentSlice.Solids,
                _extraExpansion, _minArea);

            var bridgePolys = ClipperUtil.Difference(nextSlice.Solids, expandPolys, _minArea);
            bridgePolys = CurveUtils2.FilterDegenerate(bridgePolys, _minArea);
            bridgePolys = CurveUtils2.Filter(bridgePolys, (p) => IsBridgeable(currentSlice, p));

            return bridgePolys ?? new List<GeneralPolygon2d>();
        }

        /// <summary>
        /// Check if polygon can be bridged. Currently we allow this if all hold:
        /// 1) contracting by max bridge width produces empty polygon
        /// 2) all "turning" vertices of polygon are connected to previous layer
        /// [TODO] not sure this actually guarantees that unsupported distances
        /// *between* turns are within bridge threshold...
        /// </summary>
        protected virtual bool IsBridgeable(PlanarSlice slice, GeneralPolygon2d supportPoly)
        {
            // if we inset by half bridge dist, and this doesn't completely wipe out
            // polygon, then it is too wide to bridge, somewhere
            // [TODO] this is a reasonable way to decompose into bridgeable chunks...
            double insetDelta = _maxBridgeDistance * 0.5 - _extraExpansion;
            List<GeneralPolygon2d> offset = ClipperUtil.MiterOffset(supportPoly, -insetDelta);
            if (offset != null && offset.Count > 0)
                return false;

            if (!PolyIsFullyConnected(slice, supportPoly.Outer))
                return false;

            foreach (var h in supportPoly.Holes)
            {
                if (!PolyIsFullyConnected(slice, h))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// check if all turn vertices of poly are connected ( see is_connected(vector2d) )
        /// </summary>
        protected virtual bool PolyIsFullyConnected(PlanarSlice slice, Polygon2d poly)
        {
            for (int k = 0; k < poly.VertexCount; ++k)
            {
                if (k > 0 && poly.OpeningAngleDeg(k) > 179)
                    continue;

                if (!VertexIsConnected(slice, poly[k]))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Check if position is "connected" to a solid in the slice
        /// at layer i, where connected means distance is within tolerance
        /// [TODO] I don't think this will return true if pos is inside one of the solids...
        /// </summary>
        protected virtual bool VertexIsConnected(PlanarSlice slice, Vector2d pos)
        {
            double distanceSquared = slice.DistanceSquared(pos, _bridgeToleranceSquared, true, true);
            return distanceSquared < _bridgeToleranceSquared;
        }
    }
}