using g3;
using System.Collections.Generic;

namespace Sutro.Core.Toolpathing
{
    public interface IShellsFillPolygon : ICurvesFillPolygon
    {
        List<GeneralPolygon2d> GetInnerPolygons();
    }
}