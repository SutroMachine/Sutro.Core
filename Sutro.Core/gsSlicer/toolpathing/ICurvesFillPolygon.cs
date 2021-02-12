using System.Collections.Generic;

namespace gs
{
    public interface ICurvesFillPolygon : IFillPolygon
    {
        List<FillCurveSet2d> GetFillCurves();
    }
}