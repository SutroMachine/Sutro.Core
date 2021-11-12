using g3;
using gs.FillTypes;
using System.Collections.Generic;

namespace gs
{
    public interface IFillPathScheduler2d
    {
        void AppendCurveSets(List<FillCurveSet2d> fillSets);

        SpeedHint SpeedHint { get; set; }

        Vector2d CurrentPosition { get; }

    }
}