using g3;
using System.Collections.Generic;

namespace gs
{
    public interface IShellsFillPolygon : ICurvesFillPolygon
    {
        List<GeneralPolygon2d> GetInnerPolygons();
    }
}