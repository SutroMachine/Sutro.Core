using g3;
using Sutro.Core.FillTypes;
using Sutro.Core.Toolpaths;
using System.Collections.Generic;

namespace Sutro.Core.Toolpathing
{
    public interface IFillPathScheduler2d
    {
        void AppendCurveSets(List<FillCurveSet2d> paths);

        SpeedHint SpeedHint { get; set; }

        Vector2d CurrentPosition { get; }
    }
}