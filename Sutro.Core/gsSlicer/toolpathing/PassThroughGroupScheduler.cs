using g3;
using System.Collections.Generic;

namespace gs
{
    /// <summary>
    /// This is for testing / debugging
    /// </summary>
    public class PassThroughGroupScheduler : GroupScheduler2d
    {
        public PassThroughGroupScheduler(IFillPathScheduler2d target, Vector2d startPoint) : base(target, startPoint)
        {
        }

        public override void BeginGroup()
        {
        }

        public override void EndGroup()
        {
        }

        public override bool InGroup
        {
            get { return false; }
        }

        public override void AppendCurveSets(List<FillCurveSet2d> paths)
        {
            TargetScheduler.AppendCurveSets(paths);
        }
    }
}