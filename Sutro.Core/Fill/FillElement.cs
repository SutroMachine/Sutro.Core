using g3;
using System;

namespace Sutro.Core.Fill
{
    public readonly struct FillElement<TEdge> where TEdge : IFillSegment
    {
        public Vector3d NodeStart { get; }
        public TEdge Edge { get; }
        public Vector3d NodeEnd { get; }

        public FillElement(Vector3d start, Vector3d end, TEdge edge)
        {
            NodeStart = start;
            NodeEnd = end;
            Edge = edge;
        }

        public FillElement(Vector2d start, Vector2d end, TEdge edge)
            : this(new Vector3d(start.x, start.y, 0), new Vector3d(end.x, end.y, 0), edge)
        {
        }

        public Segment2d GetSegment2d()
        {
            return new Segment2d(NodeStart.xy, NodeEnd.xy);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FillElement<TEdge> other))
                return false;

            return other.NodeStart.Equals(NodeStart) &&
                   other.NodeEnd.Equals(NodeEnd) &&
                   other.Edge.Equals(Edge);
        }

        public override int GetHashCode()
        {
            return Tuple.Create(NodeStart, NodeEnd, Edge).GetHashCode();
        }

        public void SplitElement(double splitDistanceParameterized, out FillElement<TEdge> front, out FillElement<TEdge> back)
        {
            var splitVertex = Vector3d.Lerp(NodeStart, NodeEnd, splitDistanceParameterized);
            var splitSegmentData = Edge.Split(splitDistanceParameterized);
            front = new FillElement<TEdge>(NodeStart, splitVertex, (TEdge)splitSegmentData.Item1);
            back = new FillElement<TEdge>(splitVertex, NodeEnd, (TEdge)splitSegmentData.Item2);
        }
    }
}