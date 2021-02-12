using g3;
using Sutro.Core.Fill;
using Sutro.Core.FillTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Sutro.Core.Toolpaths
{
    /// <summary>
    /// Collection of loop and curve centerlines of fill curves
    ///
    /// [TODO] support thickness variation along curves?
    /// </summary>
	public class FillCurveSet2d
    {
        public List<FillLoop> Loops =
            new List<FillLoop>();

        public List<FillCurve> Curves =
            new List<FillCurve>();

        public void Append(GeneralPolygon2d poly, IFillType fillType)
        {
            Loops.Add(new FillLoop<FillSegment>(poly.Outer.VerticesItr(false).ToList()) { FillType = fillType });
            foreach (var h in poly.Holes)
                Loops.Add(new FillLoop<FillSegment>(h.VerticesItr(false).ToList()) { FillType = fillType });
        }

        public void Append(List<GeneralPolygon2d> polys, IFillType fillType)
        {
            foreach (var p in polys)
                Append(p, fillType);
        }

        public void Append(Polygon2d poly, IFillType fillType)
        {
            Loops.Add(new FillLoop<FillSegment>(poly.VerticesItr(false).ToList()) { FillType = fillType });
        }

        public void Append(List<Polygon2d> polys, IFillType fillType)
        {
            foreach (var p in polys)
                Append(p, fillType);
        }

        public IEnumerable<FillBase> EnumerateAll()
        {
            foreach (var loop in Loops)
                yield return loop;

            foreach (var curve in Curves)
                yield return curve;
        }

        public void Append(FillLoop loop)
        {
            Loops.Add(loop);
        }

        public void Append(FillBase element)
        {
            switch (element)
            {
                case FillLoop loop:
                    Append(loop);
                    break;

                case FillCurve curve:
                    Append(curve);
                    break;

                default:
                    throw new NotImplementedException($"FillCurveSet2d.Append encountered unexpected type {element.GetType()}");
            }
        }

        public void Append(List<FillLoop> loops)
        {
            foreach (var l in loops)
                Loops.Add(l);
        }

        public void Append(FillCurve curve)
        {
            Curves.Add(curve);
        }

        public void Append(List<FillCurve> curves)
        {
            foreach (var p in curves)
                Append(p);
        }

        public void SetFillType(IFillType fillType)
        {
            foreach (var loop in Loops)
                loop.FillType = fillType;

            foreach (var curve in Curves)
                curve.FillType = fillType;
        }

        public double TotalLength()
        {
            double len = 0;
            foreach (var loop in Loops)
                len += loop.TotalLength();
            foreach (var curve in Curves)
                len += curve.TotalLength();
            return len;
        }
    }
}