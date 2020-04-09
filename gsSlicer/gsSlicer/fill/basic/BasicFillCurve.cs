﻿using g3;
using System;
using System.Collections.Generic;

namespace gs
{
    public class BasicFillCurve : FillCurveBase<BasicVertexInfo, BasicSegmentInfo>
    {
        public BasicFillCurve()
        { }

        public BasicFillCurve(IEnumerable<Vector2d> vertices)
        {
            var vertexEnumerator = vertices.GetEnumerator();
            vertexEnumerator.MoveNext();
            BeginOrAppendCurve(vertexEnumerator.Current);

            while (vertexEnumerator.MoveNext())
                BeginOrAppendCurve(vertexEnumerator.Current);
        }

        public override FillCurveBase<BasicVertexInfo, BasicSegmentInfo> CloneBare()
        {
            return new BasicFillCurve()
            {
                CustomThickness = CustomThickness,
                FillType = FillType,
            };
        }

        protected override BasicVertexInfo InterpolateVertexInfo(BasicVertexInfo vertexInfoA, BasicVertexInfo vertexInfoB, double param)
        {
            return vertexInfoA?.Interpolate(vertexInfoB, param) ?? null;
        }

        protected override Tuple<BasicSegmentInfo, BasicSegmentInfo> SplitSegmentInfo(BasicSegmentInfo segmentInfo, double param)
        {
            return segmentInfo?.Split(param) ?? new Tuple<BasicSegmentInfo, BasicSegmentInfo>(null, null);
        }
    }
}