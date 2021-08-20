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

        public LayerSupportCalculator(IPrintProfileFFF profile)
        {
            // extra offset we add to support polygons, eg to nudge them
            // in/out depending on shell layers, etc
            supportOffset = profile.Part.SupportAreaOffsetX * profile.Machine.NozzleDiamMM;

            // "insert" distance that is related to overhang angle
            //  distance = 0 means, full support
            //  distance = nozzle_diam means, 45 degrees overhang
            //  ***can't use angle w/ nozzle diam. Angle is related to layer height.
            overhangAngleDist = profile.Part.LayerHeightMM / Math.Tan(profile.Part.SupportOverhangAngleDeg * MathUtil.Deg2Rad);
        }

        public List<GeneralPolygon2d> Calculate(IReadOnlyCollection<GeneralPolygon2d> currentLayerSolids,
            IReadOnlyCollection<GeneralPolygon2d> nextLayerSolids,
            IReadOnlyCollection<GeneralPolygon2d> currentLayerBridgeArea)
        {
            // expand this layer and subtract from next layer. leftovers are
            // what needs to be supported on next layer.
            List<GeneralPolygon2d> expandPolys = ClipperUtil.MiterOffset(currentLayerSolids, overhangAngleDist);
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