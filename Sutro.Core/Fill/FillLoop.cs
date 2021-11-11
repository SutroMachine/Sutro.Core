using g3;
using System;
using System.Collections.Generic;

namespace Sutro.Core.Fill
{
    public abstract class FillLoop : FillBase
    {
        public abstract FillLoop RollToVertex(int index);

        public abstract FillLoop RollBetweenVertices(ElementLocation location, double tolerance = 0.001);

        public abstract FillLoop Reversed();

        public abstract bool IsClockwise();

        public abstract IEnumerable<Vector2d> Vertices(bool repeatFirst = false);

        public abstract List<FillCurve> SplitAtDistances(IEnumerable<double> splitDistances, bool joinEnds);
    }

    /// <summary>
    /// Additive polygon fill curve
    /// </summary>
    public class FillLoop<TSegmentInfo> : FillLoop
        where TSegmentInfo : IFillSegment, new()
    {
        private readonly FillElementList<TSegmentInfo> elementsList = new FillElementList<TSegmentInfo>();
        public IReadOnlyList<FillElement<TSegmentInfo>> Elements => elementsList.Elements;

        protected FillLoop()
        {
        }

        public FillLoop<TSegmentInfo> CloneBare()
        {
            var loop = new FillLoop<TSegmentInfo>();
            loop.CopyProperties(this);
            return loop;
        }

        public FillCurve<TSegmentInfo> CloneBareAsCurve()
        {
            var curve = new FillCurve<TSegmentInfo>();
            curve.CopyProperties(this);
            return curve;
        }

        public override bool IsClockwise()
        {
            // Note: Could cache or otherwise optimize this computation
            var poly = new Polygon2d(Vertices(false));
            return poly.IsClockwise;
        }

        public override Vector2d Entry => elementsList.Elements[0].NodeStart.xy;
        public override Vector2d Exit => Entry;
        public override int ElementCount => elementsList.Elements.Count;

        public FillLoop(IList<Vector2d> vertices)
        {
            for (int i = 1; i < vertices.Count; i++)
            {
                elementsList.Add(new FillElement<TSegmentInfo>(vertices[i - 1], vertices[i], new TSegmentInfo()));
            }
            elementsList.Add(new FillElement<TSegmentInfo>(vertices[^1], vertices[0], new TSegmentInfo()));
        }

        public FillLoop(IEnumerable<FillElement<TSegmentInfo>> elements)
        {
            foreach (var e in elements)
                elementsList.Add(e);
        }

        public override FillLoop RollToVertex(int index)
        {
            // TODO: Add range checking for startIndex
            var rolledLoop = new FillLoop<TSegmentInfo>();
            rolledLoop.CopyProperties(this);

            for (int i = 0; i < elementsList.Elements.Count; i++)
            {
                rolledLoop.elementsList.Add(elementsList.Elements[(i + index) % elementsList.Elements.Count]);
            }

            return rolledLoop;
        }

        public override FillLoop RollBetweenVertices(ElementLocation location, double tolerance = 0.001)
        {
            if (!ElementShouldSplit(location.ParameterizedDistance, tolerance, elementsList.Elements[location.Index].GetSegment2d().Length))
            {
                return RollToVertex(IdentifyClosestVertex(location.Index, location.ParameterizedDistance));
            }

            var elementToSplit = elementsList.Elements[location.Index];

            var interpolatedVertex = Vector3d.Lerp(elementToSplit.NodeStart, elementToSplit.NodeEnd, location.ParameterizedDistance);

            var splitSegmentData = SplitSegment(location.ParameterizedDistance, elementToSplit);

            var rolledElements = CreateRolledElements(location.Index, interpolatedVertex, elementToSplit, splitSegmentData);

            var rolledLoop = new FillLoop<TSegmentInfo>(rolledElements);
            rolledLoop.CopyProperties(this);
            return rolledLoop;
        }

        private List<FillElement<TSegmentInfo>> CreateRolledElements(int elementIndex, Vector3d interpolatedVertex, FillElement<TSegmentInfo> elementToSplit,
            Tuple<IFillSegment, IFillSegment> splitSegmentData)
        {
            var rolledElements = new List<FillElement<TSegmentInfo>>(elementsList.Elements.Count + 1);

            // Add the second half of the split element
            rolledElements.Add(new FillElement<TSegmentInfo>(
                interpolatedVertex,
                elementToSplit.NodeEnd,
                (TSegmentInfo)splitSegmentData.Item2));

            // Add all elements after the split element
            for (int i = elementIndex + 1; i < elementsList.Elements.Count; ++i)
                rolledElements.Add(elementsList.Elements[i]);

            // Add all elements before the split element
            for (int i = 0; i < elementIndex; ++i)
                rolledElements.Add(elementsList.Elements[i]);

            // Add the first half of the split element
            rolledElements.Add(new FillElement<TSegmentInfo>(
                elementToSplit.NodeStart,
                interpolatedVertex,
                (TSegmentInfo)splitSegmentData.Item1));
            return rolledElements;
        }

        private static Tuple<IFillSegment, IFillSegment> SplitSegment(double parameterizedDistance, FillElement<TSegmentInfo> element)
        {
            return element.Edge == null
                ? Tuple.Create((IFillSegment)new TSegmentInfo(), (IFillSegment)new TSegmentInfo())
                : element.Edge.Split(parameterizedDistance);
        }

        private int IdentifyClosestVertex(int elementIndex, double elementParameterizedDistance)
        {
            if (elementParameterizedDistance > 0.5)
            {
                ++elementIndex;
                if (elementIndex >= elementsList.Elements.Count)
                {
                    elementIndex = 0;
                }
            }

            return elementIndex;
        }

        private static bool ElementShouldSplit(double parameterizedSplitDistance, double tolerance, double segmentLength)
        {
            double toleranceParameterized = tolerance / segmentLength;
            return parameterizedSplitDistance > toleranceParameterized && parameterizedSplitDistance < 1 - toleranceParameterized;
        }

        public FillCurve<TSegmentInfo> ConvertToCurve()
        {
            var curve = new FillCurve<TSegmentInfo>(elementsList.Elements);
            curve.CopyProperties(this);
            return curve;
        }

        public override FillLoop Reversed()
        {
            var loop = new FillLoop<TSegmentInfo>(elementsList.Reversed());
            loop.CopyProperties(this);
            return loop;
        }

        public override IEnumerable<Vector2d> Vertices(bool repeatFirst = false)
        {
            foreach (var edge in elementsList.Elements)
                yield return edge.NodeStart.xy;

            if (repeatFirst)
                yield return elementsList.Elements[0].NodeStart.xy;
        }

        public List<FillCurve<TSegmentInfo>> SplitAtDistances(double[] splitDistances, bool joinFirstAndLast = false)
        {
            var elementGroups = FillSplitter<TSegmentInfo>.SplitAtDistances(splitDistances, elementsList.Elements);

            if (joinFirstAndLast)
            {
                var firstCurve = elementGroups[0];
                elementGroups.RemoveAt(0);
                elementGroups[^1].AddRange(firstCurve);
            }

            var curves = new List<FillCurve<TSegmentInfo>>();
            foreach (var elementGroup in elementGroups)
            {
                var curve = new FillCurve<TSegmentInfo>(elementGroup);
                curve.CopyProperties(this);
                curves.Add(curve);
            }
            return curves;
        }

        public override List<FillCurve> SplitAtDistances(IEnumerable<double> splitDistances, bool joinEnds)
        {
            var elementGroups = FillSplitter<TSegmentInfo>.SplitAtDistances(splitDistances, elementsList.Elements);

            var curves = new List<FillCurve>();
            foreach (var elementGroup in elementGroups)
            {
                var curve = new FillCurve<TSegmentInfo>(elementGroup);
                curve.CopyProperties(this);
                curves.Add(curve);
            }

            if (joinEnds)
            {
                var firstCurve = curves[0];
                curves.RemoveAt(0);
                curves[^1].Extend(firstCurve);
            }

            return curves;
        }

        public override Vector3d GetVertex(int index)
        {
            return elementsList.GetVertex(index);
        }

        public override Segment2d GetSegment2d(int index)
        {
            return elementsList.GetSegment2d(index);
        }

        public override double TotalLength()
        {
            return elementsList.TotalLength();
        }

        public override double FindClosestElementToPoint(Vector2d point, out ElementLocation location)
        {
            return elementsList.FindClosestElementToPoint(point, out location);
        }
    }
}