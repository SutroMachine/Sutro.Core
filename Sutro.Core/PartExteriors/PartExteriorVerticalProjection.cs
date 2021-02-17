using g3;
using gs;
using Sutro.Core.Slicing;
using System.Collections.Generic;
using System.Threading;

namespace Sutro.Core.PartExteriors
{
    public class PartExteriorVerticalProjection : IPartExterior
    {
        protected List<GeneralPolygon2d>[] layerFloorAreas;
        protected List<GeneralPolygon2d>[] layerRoofAreas;
        private readonly int floorLayers;
        private readonly double minArea;
        private readonly double overhangAllowance;
        private readonly int roofLayers;
        private readonly PlanarSliceStack slices;

        public PartExteriorVerticalProjection(PlanarSliceStack slices,
                                              double minArea, double overhangAllowance,
                                              int floorLayers, int roofLayers)
        {
            this.slices = slices;
            this.minArea = minArea;
            this.overhangAllowance = overhangAllowance;
            this.floorLayers = floorLayers;
            this.roofLayers = roofLayers;
        }

        public List<GeneralPolygon2d> GetExteriorRegions(int layerIndex,
            IReadOnlyCollection<GeneralPolygon2d> subject)
        {
            var roofPolys = ClipperUtil.Difference(subject, layerRoofAreas[layerIndex], minArea);
            var floorPolys = ClipperUtil.Difference(subject, layerFloorAreas[layerIndex], minArea);

            var exteriorRegions = ClipperUtil.Union(roofPolys, floorPolys, minArea);

            if (exteriorRegions == null)
                exteriorRegions = new List<GeneralPolygon2d>();

            return exteriorRegions;
        }

        /// <summary>
        /// compute all the roof and floor areas for the entire stack, in parallel
        /// </summary>
        public virtual void Initialize(CancellationToken? cancel)
        {
            int nLayers = slices.Count;
            layerRoofAreas = new List<GeneralPolygon2d>[nLayers];
            layerFloorAreas = new List<GeneralPolygon2d>[nLayers];

            //int start_layer = Math.Max(0, Settings.Part.LayerRangeFilter.a);
            //int end_layer = Math.Min(nLayers - 1, Settings.Part.LayerRangeFilter.b);
            //Interval1i solveInterval = new Interval1i(start_layer, end_layer);
            var solveInterval = new Interval1i(0, nLayers - 1);

#if DEBUG
            for (int layerIndex = solveInterval.a; layerIndex <= solveInterval.b; ++layerIndex)
#else
            gParallel.ForEach(solveInterval, (layerIndex) =>
#endif
            {
                if (cancel?.IsCancellationRequested ?? false) return;
                bool is_infill = layerIndex >= floorLayers && layerIndex < nLayers - roofLayers;

                if (is_infill)
                {
                    if (roofLayers > 0)
                    {
                        layerRoofAreas[layerIndex] = FindLayerRoofAreas(layerIndex);
                    }
                    else
                    {
                        layerRoofAreas[layerIndex] = FindLayerRoofAreas(layerIndex - 1);     // will return "our" layer
                    }
                    if (floorLayers > 0)
                    {
                        layerFloorAreas[layerIndex] = FindLayerFloorAreas(layerIndex);
                    }
                    else
                    {
                        layerFloorAreas[layerIndex] = FindLayerFloorAreas(layerIndex + 1);   // will return "our" layer
                    }
                }
                else
                {
                    layerRoofAreas[layerIndex] = new List<GeneralPolygon2d>();
                    layerFloorAreas[layerIndex] = new List<GeneralPolygon2d>();
                }

                //count_progress_step();
#if DEBUG
            }
#else
            });
#endif
        }

        /// <summary>
        /// construct region that needs to be solid for "floors"
        /// </summary>
        protected virtual List<GeneralPolygon2d> FindLayerFloorAreas(int layerIndex)
        {
            List<GeneralPolygon2d> floorCover = new List<GeneralPolygon2d>();

            // TODO: Shrink?
            floorCover.AddRange(slices[layerIndex - 1].Solids);

            // If we want > 1 floor layer, we need to look further back.
            for (int k = 2; k <= floorLayers; ++k)
            {
                int ri = layerIndex - k;
                if (ri >= 0)
                {
                    List<GeneralPolygon2d> infillN = new List<GeneralPolygon2d>();

                    // TODO: Shrink?
                    infillN.AddRange(slices[ri].Solids);
                    floorCover = ClipperUtil.Intersection(floorCover, infillN, minArea);
                }
            }

            // add overhang allowance.
            var result = ClipperUtil.MiterOffset(floorCover, overhangAllowance, minArea);
            return result;
        }

        /// <summary>
        /// construct region that needs to be solid for "roofs".
        /// This is the intersection of infill polygons for the next N layers.
        /// </summary>
        protected virtual List<GeneralPolygon2d> FindLayerRoofAreas(int layerIndex)
        {
            List<GeneralPolygon2d> roofCover = new List<GeneralPolygon2d>();

            // TODO: Shrink?
            roofCover.AddRange(slices[layerIndex + 1].Solids);

            // If we want > 1 roof layer, we need to look further ahead.
            // The full area we need to print as "roof" is the infill minus
            // the intersection of the infill areas above
            for (int k = 2; k <= roofLayers; ++k)
            {
                int ri = layerIndex + k;
                if (ri < slices.Count)
                {
                    List<GeneralPolygon2d> infillN = new List<GeneralPolygon2d>();

                    // TODO: Shrink?
                    infillN.AddRange(slices[ri].Solids);

                    roofCover = ClipperUtil.Intersection(roofCover, infillN, minArea);
                }
            }

            // add overhang allowance. Technically any non-vertical surface will result in
            // non-empty roof regions. However we do not need to explicitly support roofs
            // until they are "too horizontal".
            var result = ClipperUtil.MiterOffset(roofCover, overhangAllowance, minArea);
            return result;
        }
    }
}