using Sutro.Core.Toolpaths;
using System.Collections.Generic;

namespace Sutro.Core.Toolpathing
{
    public interface ICurvesFillPolygon : IFillPolygon
    {
        List<FillCurveSet2d> GetFillCurves();
    }
}