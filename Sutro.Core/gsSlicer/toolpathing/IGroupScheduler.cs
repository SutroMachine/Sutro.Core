using System.Collections.Generic;

namespace gs
{
    public interface IGroupScheduler
    {
        bool InGroup { get; }

        void AppendCurveSets(List<FillCurveSet2d> paths);

        void BeginGroup();

        void EndGroup();
    }
}