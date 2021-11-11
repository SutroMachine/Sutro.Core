using g3;
using System;
using System.Collections.Generic;

namespace Sutro.Core.Fill
{
    public abstract class FillCurve : FillBase
    {
        public abstract List<FillCurve> SplitAtDistances(IEnumerable<double> splitDistances);

        public abstract FillCurve Reversed();

        public abstract FillCurve TrimFront(double trimDistance);

        public abstract FillCurve TrimBack(double trimDistance);

        public abstract FillCurve TrimFrontAndBack(double trimDistanceFront, double? trimDistanceBack = null);

        public abstract IEnumerable<Vector2d> Vertices();

        public abstract IEnumerable<Vector3d> Vertices3d();

        public abstract void Extend(FillCurve other, double vertexComparisonTolerance = 1e-6);
    }

    /// <summary>
    /// Additive polyline fill curve
    /// </summary>
    public class FillCurve<TSegmentInfo> : FillCurve
        where TSegmentInfo : IFillSegment, new()
    {
        public override Vector2d Entry => elementsList.Elements[0].NodeStart.xy;
        public override Vector2d Exit => elementsList.Elements[^1].NodeEnd.xy;
        public override int ElementCount => elementsList.Elements.Count;

        private readonly FillElementList<TSegmentInfo> elementsList = new FillElementList<TSegmentInfo>();
        public IReadOnlyList<FillElement<TSegmentInfo>> Elements => elementsList.Elements;

        public override double TotalLength()
        {
            return elementsList.TotalLength();
        }

        public override double FindClosestElementToPoint(Vector2d point, out ElementLocation location)
        {
            return elementsList.FindClosestElementToPoint(point, out location);
        }

        public FillCurve<TSegmentInfo> CloneBare()
        {
            var curve = new FillCurve<TSegmentInfo>();
            curve.CopyProperties(this);
            return curve;
        }

        public FillCurve()
        {
        }

        public FillCurve(PolyLine2d polyline) : this(polyline.Vertices)
        {
        }

        public FillCurve(IList<Vector2d> vertices)
        {
            for (int i = 1; i < vertices.Count; i++)
            {
                elementsList.Add(new FillElement<TSegmentInfo>(vertices[i - 1], vertices[i], new TSegmentInfo()));
            }
        }

        public FillCurve(IEnumerable<FillElement<TSegmentInfo>> elements)
        {
            // Note: may want to add continuity checking here for start/end vertices
            foreach (var e in elements)
                elementsList.Add(e);
        }

        private Vector3d? firstPoint = null;

        public void BeginCurve(Vector3d pt)
        {
            if (elementsList.Elements.Count > 0 || firstPoint != null)
                throw new MethodAccessException("BeginCurve called more than once.");
            firstPoint = pt;
        }

        public void BeginCurve(Vector2d pt)
        {
            BeginCurve(new Vector3d(pt.x, pt.y, 0));
        }

        public void AddToCurve(Vector3d pt, TSegmentInfo segmentInfo)
        {
            if (elementsList.Elements.Count > 0)
            {
                elementsList.Add(new FillElement<TSegmentInfo>(elementsList.Elements[^1].NodeEnd, pt, segmentInfo));
            }
            else if (firstPoint.HasValue)
            {
                elementsList.Add(new FillElement<TSegmentInfo>(firstPoint.Value, pt, segmentInfo));
            }
            else
            {
                throw new MethodAccessException("AddToCurve called before BeginCurve.");
            }
        }

        public void AddToCurve(Vector2d pt, TSegmentInfo segmentInfo)
        {
            AddToCurve(new Vector3d(pt.x, pt.y, 0), segmentInfo);
        }

        public void AddToCurve(Vector3d pt)
        {
            AddToCurve(pt, new TSegmentInfo());
        }

        public void AddToCurve(Vector2d pt)
        {
            AddToCurve(new Vector3d(pt.x, pt.y, 0), new TSegmentInfo());
        }

        public FillLoop<TSegmentInfo> CloseCurve(TSegmentInfo segmentInfo)
        {
            var loopElements = new List<FillElement<TSegmentInfo>>(elementsList.Elements.Count + 1);
            foreach (var e in elementsList.Elements)
                loopElements.Add(e);
            loopElements.Add(new FillElement<TSegmentInfo>(loopElements[^1].NodeEnd, loopElements[0].NodeStart, segmentInfo));
            var loop = new FillLoop<TSegmentInfo>(loopElements);
            loop.CopyProperties(this);
            return loop;
        }

        public FillLoop<TSegmentInfo> CloseCurve()
        {
            return CloseCurve(new TSegmentInfo());
        }

        public override IEnumerable<Vector2d> Vertices()
        {
            yield return elementsList.Elements[0].NodeStart.xy;
            foreach (var edge in elementsList.Elements)
                yield return edge.NodeEnd.xy;
        }

        public override IEnumerable<Vector3d> Vertices3d()
        {
            yield return elementsList.Elements[0].NodeStart;
            foreach (var edge in elementsList.Elements)
                yield return edge.NodeEnd;
        }

        public override FillCurve Reversed()
        {
            var curve = new FillCurve<TSegmentInfo>(elementsList.Reversed());
            curve.CopyProperties(this);
            return curve;
        }

        public override void Extend(FillCurve other, double vertexComparisonTolerance = 1e-6)
        {
            if (!(other is FillCurve<TSegmentInfo> o))
                throw new ArgumentException($"Cannot merge FillCurves with different segment types: {GetType()} and {other.GetType()}");
            Extend(o.Elements);
        }

        public void Extend(IEnumerable<FillElement<TSegmentInfo>> elements, double vertexComparisonTolerance = 1e-6)
        {
            var enumerator = elements.GetEnumerator();
            enumerator.MoveNext();

            if (!enumerator.Current.NodeStart.EpsilonEqual(elementsList.Elements[^1].NodeEnd, vertexComparisonTolerance))
            {
                throw new ArgumentException("Can only extend with a FillCurve that starts where this FillCurve ends.");
            }
            elementsList.Add(enumerator.Current);
            while (enumerator.MoveNext())
            {
                elementsList.Add(enumerator.Current);
            }
            enumerator.Dispose();
        }

        public override List<FillCurve> SplitAtDistances(IEnumerable<double> splitDistances)
        {
            var elementGroups = FillSplitter<TSegmentInfo>.SplitAtDistances(splitDistances, elementsList.Elements);

            var curves = new List<FillCurve>();
            foreach (var elementGroup in elementGroups)
            {
                var curve = new FillCurve<TSegmentInfo>(elementGroup);
                curve.CopyProperties(this);
                curves.Add(curve);
            }
            return curves;
        }

        public override FillCurve TrimFront(double trimDistance)
        {
            ValidateTrimDistance(trimDistance, TotalLength());
            return SplitAtDistances(new double[] { trimDistance })[1];
        }

        public override FillCurve TrimBack(double trimDistance)
        {
            double totalLength = TotalLength();
            double trimLocation = totalLength - trimDistance;
            ValidateTrimDistance(trimLocation, totalLength);
            return SplitAtDistances(new double[] { trimLocation })[0];
        }

        public override FillCurve TrimFrontAndBack(double trimDistanceFront, double? trimDistanceBack = null)
        {
            double totalLength = TotalLength();
            double trimLocationBack = totalLength - (trimDistanceBack ?? trimDistanceFront);

            ValidateTrimDistance(trimDistanceFront, totalLength);
            ValidateTrimDistance(trimLocationBack, totalLength);

            if (trimLocationBack < trimDistanceFront)
                throw new ArgumentException("Combined trim amounts are greater than curve length.");

            return SplitAtDistances(new double[] { trimDistanceFront, trimLocationBack })[1];
        }

        private void ValidateTrimDistance(double trimLocation, double totalLength)
        {
            if (trimLocation > totalLength)
                throw new ArgumentException("Trim location must be less than total length.");

            if (trimLocation <= 0)
                throw new ArgumentException("Trim location must be greater than 0.");
        }

        public override Vector3d GetVertex(int index)
        {
            return elementsList.GetVertex(index);
        }

        public override Segment2d GetSegment2d(int index)
        {
            return elementsList.GetSegment2d(index);
        }
    }
}