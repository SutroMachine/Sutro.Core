using g3;
using System.Collections.Generic;

namespace gs
{
    /// <summary>
    /// This is for testing / debugging
    /// </summary>
    public class PassThroughGroupScheduler : IGroupScheduler
    {
        private readonly IFillPathScheduler2d target;

        public PassThroughGroupScheduler(IFillPathScheduler2d target)
        {
            this.target = target;
        }

        public void BeginGroup()
        {
            // Do nothing
        }

        public void EndGroup()
        {
            // Do nothing
        }

        public bool InGroup
        {
            get { return false; }
        }

        public void AppendCurveSets(List<FillCurveSet2d> paths)
        {
            target.AppendCurveSets(paths);
        }
    }
}