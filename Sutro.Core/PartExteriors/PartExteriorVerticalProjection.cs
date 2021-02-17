using g3;
using gs;
using Sutro.Core.Slicing;
using Sutro.Core.Utility;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Sutro.Core.PartExteriors
{
    public class PartExteriorVerticalProjection : IPartExterior
    {
        private readonly int floorLayerCount;
        private readonly int roofLayerCount;

        private readonly double minArea;
        private readonly double anchorDistance;

        private readonly PlanarSliceStack slices;
        private readonly ConcurrentDictionary<int, List<GeneralPolygon2d>> interiors;

        public PartExteriorVerticalProjection(PlanarSliceStack slices,
                                              double minArea, double anchorDistance,
                                              int floorLayerCount, int roofLayerCount)
        {
            this.slices = slices;

            this.floorLayerCount = floorLayerCount;
            this.roofLayerCount = roofLayerCount;

            this.minArea = minArea;
            this.anchorDistance = anchorDistance;

            // Initialize the concurrent dictionary collection
            interiors = new ConcurrentDictionary<int, List<GeneralPolygon2d>>();
        }

        public List<GeneralPolygon2d> GetExteriorRegions(int layerIndex,
            IReadOnlyCollection<GeneralPolygon2d> subject)
        {
            return ClipperUtil.Difference(subject, interiors[layerIndex], minArea);
        }

        public List<GeneralPolygon2d> CreateLayerInterior(int layerIndex)
        {
            if (!LayerHasInfill(layerIndex))
                return new List<GeneralPolygon2d>();

            var intersection = slices[layerIndex].Solids;

            foreach (var adjacentLayer in EnumerateRoofLayers(layerIndex).Concat(EnumerateFloorLayers(layerIndex)))
            {
                intersection = ClipperUtil.Intersection(intersection, adjacentLayer, minArea);
            }

            return SubtractAnchorDistance(intersection) ?? new List<GeneralPolygon2d>();
        }

        /// <summary>
        /// compute all the roof and floor areas for the entire stack, in parallel
        /// </summary>
        public virtual void Initialize(CancellationToken? cancel)
        {
            var solveInterval = new Interval1i(0, slices.Count - 1);

            Parallel.ForEach(solveInterval, (layerIndex, i) =>
            {
                if (cancel?.IsCancellationRequested ?? false) return;
                interiors[layerIndex] = CreateLayerInterior(layerIndex);
            });
        }

        private bool LayerHasInfill(int layerIndex)
        {
            return layerIndex >= floorLayerCount && layerIndex < slices.Count - roofLayerCount;
        }

        /// <summary>
        /// Construct region that needs to be solid for "floors".
        /// This is the intersection of solids for the previous N layers.
        /// </summary>
        protected virtual IEnumerable<List<GeneralPolygon2d>> EnumerateFloorLayers(int layerIndex)
        {
            // If we want > 1 floor layer, we need to look further back.
            for (int floorLayerOffset = 1; floorLayerOffset <= floorLayerCount; ++floorLayerOffset)
            {
                int floorLayerIndex = layerIndex - floorLayerOffset;

                if (floorLayerIndex >= 0)
                {
                    yield return slices[floorLayerIndex].Solids;
                }
            }
        }

        private List<GeneralPolygon2d> SubtractAnchorDistance(List<GeneralPolygon2d> cover)
        {
            return ClipperUtil.MiterOffset(cover, -anchorDistance, minArea);
        }

        /// <summary>
        /// Construct region that needs to be solid for "roofs".
        /// This is the intersection of solids for the next N layers.
        /// </summary>
        protected virtual IEnumerable<List<GeneralPolygon2d>> EnumerateRoofLayers(int layerIndex)
        {
            // If we want > 1 roof layer, we need to look further ahead.
            // The full area we need to print as "roof" is the infill minus
            // the intersection of the infill areas above
            for (int roofOffset = 1; roofOffset <= roofLayerCount; ++roofOffset)
            {
                int roofLayerIndex = layerIndex + roofOffset;
                if (roofLayerIndex < slices.Count)
                {
                    yield return slices[roofLayerIndex].Solids;
                }
            }
        }
    }
}