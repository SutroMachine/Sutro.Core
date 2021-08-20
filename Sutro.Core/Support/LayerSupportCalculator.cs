using g3;
using gs;
using Sutro.Core.Settings;
using System;
using System.Collections.Generic;

namespace Sutro.Core.Support
{
    public class LayerSupportCalculator
    {
        private readonly double supportOffset;
        private readonly double overhangAngleDist;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="supportOffset">Extra offset to add to support polygons, eg to nudge them in/out depending on number of shell layers, etc.</param>
        /// <param name="overhangAngleDist">Extra offset to add to support polygons, eg to nudge them in/out depending on number of shell layers, etc.</param>
        public LayerSupportCalculator(double supportOffset, double overhangAngleDist)
        {
            this.supportOffset = supportOffset;
            this.overhangAngleDist = overhangAngleDist;
        }

        public List<GeneralPolygon2d> Calculate(IReadOnlyCollection<GeneralPolygon2d> currentLayerSolids,
            IReadOnlyCollection<GeneralPolygon2d> nextLayerSolids,
            IReadOnlyCollection<GeneralPolygon2d> currentLayerBridgeArea)
        {
            // Expand the current layer and subtract from layer above; leftovers are
            // the regions of the layer above that need to be supported.
            var expandPolys = ClipperUtil.MiterOffset(currentLayerSolids, overhangAngleDist);
            var supportPolys = ClipperUtil.Difference(nextLayerSolids, expandPolys);

            // subtract regions we are going to bridge
            if (currentLayerBridgeArea.Count > 0)
            {
                supportPolys = ClipperUtil.Difference(supportPolys, currentLayerBridgeArea);
            }

            // if we have an support inset/outset, apply it here.
            // for insets the poly may disappear, in that case we
            // keep the original poly.
            // [TODO] handle partial-disappears
            if (supportOffset != 0)
            {
                List<GeneralPolygon2d> offsetPolys = new List<GeneralPolygon2d>();
                foreach (var poly in supportPolys)
                {
                    List<GeneralPolygon2d> offset = ClipperUtil.MiterOffset(poly, supportOffset);
                    // if offset is empty, use original poly
                    if (offset.Count == 0)
                    {
                        offsetPolys.Add(poly);
                    }
                    else
                    {
                        offsetPolys.AddRange(offset);
                    }
                }
                supportPolys = offsetPolys;
            }
            return supportPolys;
        }
    }
}