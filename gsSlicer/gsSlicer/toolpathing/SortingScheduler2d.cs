﻿using g3;
using System.Collections.Generic;

namespace gs
{
    /// <summary>
    /// SortingScheduler is a 2d path scheduler that collects up a set of
    /// curves and loops, and then tries to find an ordering that will be
    /// more efficient (ie minimizes travel). You call AppendPaths() like
    /// any other scheduler, and then SortAndAppendTo(real_scheduler) to
    /// actually emit the paths.
    ///
    /// Requires you to provide an input point, and .OutPoint is updated
    /// after you do the sort.
    ///
    /// [TODO] loop handling could be improved
    /// [TODO] do bounding-box distance checks to early-out
    /// [TODO] less O(N^2) business?
    ///
    /// </summary>
    public class SortingScheduler2d : IFillPathScheduler2d
    {
        public SchedulerSpeedHint SpeedHint { get; set; }

        /// <summary>
        /// Final point in the output paths, computed by SortAndAppendTo()
        /// </summary>
        public Vector2d OutPoint;

        protected class PathItem
        {
            public SchedulerSpeedHint speedHint;
        }

        protected class PathLoop : PathItem
        {
            public BasicFillLoop curve;
            public bool reverse = false;
        }

        protected List<PathLoop> Loops = new List<PathLoop>();

        protected class PathSpan : PathItem
        {
            public BasicFillCurve curve;
            public bool reverse = false;
        }

        protected List<PathSpan> Spans = new List<PathSpan>();

        public virtual void AppendCurveSets(List<FillCurveSet2d> paths)
        {
            foreach (FillCurveSet2d polySet in paths)
            {
                foreach (BasicFillLoop loop in polySet.Loops)
                    Loops.Add(new PathLoop() { curve = loop, speedHint = SpeedHint });

                foreach (BasicFillCurve curve in polySet.Curves)
                    Spans.Add(new PathSpan() { curve = curve, speedHint = SpeedHint });
            }
        }

        public virtual void SortAndAppendTo(Vector2d startPoint, IFillPathScheduler2d scheduler)
        {
            var saveHint = scheduler.SpeedHint;
            OutPoint = startPoint;

            List<Index3i> sorted = find_short_path_v1(startPoint);
            foreach (Index3i idx in sorted)
            {
                FillCurveSet2d paths = new FillCurveSet2d();

                SchedulerSpeedHint pathHint = SchedulerSpeedHint.Default;
                if (idx.a == 0)
                { // loop
                    PathLoop loop = Loops[idx.b];
                    pathHint = loop.speedHint;
                    if (idx.c != 0)
                    {
                        int iStart = idx.c;
                        BasicFillLoop o = new BasicFillLoop();
                        int N = loop.curve.VertexCount;
                        o.BeginLoop(loop.curve[iStart]);
                        for (int i = 1; i < N; ++i)
                        {
                            o.AddToLoop(loop.curve[(i + iStart) % N]);
                        }
                        o.CloseLoop();
                        o.FillType = loop.curve.FillType;
                        paths.Append(o);
                        OutPoint = o[0];
                    }
                    else
                    {
                        paths.Append(loop.curve);
                        OutPoint = loop.curve[0];
                    }
                }
                else
                {  // span
                    PathSpan span = Spans[idx.b];
                    if (idx.c == 1)
                        span.curve.Reverse();
                    paths.Append(span.curve);
                    OutPoint = span.curve.End;
                    pathHint = span.speedHint;
                }

                scheduler.SpeedHint = pathHint;
                scheduler.AppendCurveSets(new List<FillCurveSet2d>() { paths });
            }

            scheduler.SpeedHint = saveHint;
        }

        // [TODO] make this work. need more matrices?
        //  not even sure this makes sense if we are doing greedy algo, we
        //  will never compute a pairwise distance more than once, will we??
        //DenseMatrix spanSS;
        //DenseMatrix spanSE;

        protected virtual List<Index3i> find_short_path_v1(Vector2d vStart)
        {
            int N = Spans.Count;
            int M = Loops.Count;

            HashSet<Index2i> remaining = new HashSet<Index2i>();
            List<Index3i> order = new List<Index3i>();

            for (int i = 0; i < N; ++i)
            {
                remaining.Add(new Index2i(1, i));
            }
            for (int i = 0; i < M; ++i)
            {
                remaining.Add(new Index2i(0, i));
            }
            if (remaining.Count == 0)
                return order;

            Index3i start_idx = find_nearest(vStart, remaining);
            order.Add(start_idx);
            remaining.Remove(new Index2i(start_idx.a, start_idx.b));

            Index3i prev = start_idx;
            while (remaining.Count > 0)
            {
                Index3i next = find_nearest(prev, remaining);
                if (next == Index3i.Max)
                    break;
                order.Add(next);
                remaining.Remove(new Index2i(next.a, next.b));
                prev = next;
            }

            // handle fails
            foreach (Index2i idx in remaining)
            {
                order.Add(new Index3i(idx.a, idx.b, 0));
            }

            return order;
        }

        protected virtual Index3i find_nearest(Index3i from, HashSet<Index2i> remaining)
        {
            Vector2d pt = get_point(from);

            double nearest_sqr = double.MaxValue;
            Index3i nearest_idx = Index3i.Max;
            foreach (Index2i idx in remaining)
            {
                if (idx.a == 0)
                { // loop
                    PathLoop loop = Loops[idx.b];
                    int iNearSeg; double nearSegT;
                    double d_sqr = loop.curve.DistanceSquared(pt, out iNearSeg, out nearSegT);
                    if (d_sqr < nearest_sqr)
                    {
                        nearest_sqr = d_sqr;
                        nearest_idx = new Index3i(idx.a, idx.b, iNearSeg);
                    }
                }
                else
                {  // span
                    PathSpan span = Spans[idx.b];
                    double start_d = span.curve.Start.DistanceSquared(pt);
                    if (start_d < nearest_sqr)
                    {
                        nearest_sqr = start_d;
                        nearest_idx = new Index3i(idx.a, idx.b, 0);
                    }
                    double end_d = span.curve.End.DistanceSquared(pt);
                    if (end_d < nearest_sqr)
                    {
                        nearest_sqr = end_d;
                        nearest_idx = new Index3i(idx.a, idx.b, 1);
                    }
                }
            }

            return nearest_idx;
        }

        protected virtual Index3i find_nearest(Vector2d pt, HashSet<Index2i> remaining)
        {
            double nearest_sqr = double.MaxValue;
            Index3i nearest_idx = Index3i.Max;
            foreach (Index2i idx in remaining)
            {
                if (idx.a == 0)
                { // loop
                    PathLoop loop = Loops[idx.b];
                    int iNearSeg; double nearSegT;
                    double d_sqr = loop.curve.DistanceSquared(pt, out iNearSeg, out nearSegT);
                    if (d_sqr < nearest_sqr)
                    {
                        nearest_sqr = d_sqr;
                        nearest_idx = new Index3i(idx.a, idx.b, iNearSeg);
                    }
                }
                else
                {  // span
                    PathSpan span = Spans[idx.b];
                    double start_d = span.curve.Start.DistanceSquared(pt);
                    if (start_d < nearest_sqr)
                    {
                        nearest_sqr = start_d;
                        nearest_idx = new Index3i(idx.a, idx.b, 0);
                    }
                    double end_d = span.curve.End.DistanceSquared(pt);
                    if (end_d < nearest_sqr)
                    {
                        nearest_sqr = end_d;
                        nearest_idx = new Index3i(idx.a, idx.b, 1);
                    }
                }
            }

            return nearest_idx;
        }

        protected virtual Vector2d get_point(Index3i idx)
        {
            if (idx.a == 0)
            { // loop
                PathLoop loop = Loops[idx.b];
                return loop.curve.SegmentAfterIndex(idx.c).Center;
            }
            else
            {  // span
                PathSpan span = Spans[idx.b];

                // [GDM] Reversed this logic 2019.10.23; by my thinking:
                // - if the curve ISN'T reversed, the exit point should be the end
                // - if the curve IS reversed, the exit point should be the start
                return (idx.c == 0) ? span.curve.End : span.curve.Start;
            }
        }

        //void precompute_distances()
        //{
        //    int N = Spans.Count;
        //    spanSS = new DenseMatrix(N, N);
        //    spanSE = new DenseMatrix(N, N);
        //    for ( int i = 0; i < N; ++i ) {
        //        Vector2d vS = Spans[i].curve.Start;
        //        for ( int j = i+1; j < N; ++j ) {
        //            spanSS[i, j] = vS.Distance(Spans[j].curve.Start);
        //            spanSE[i, j] = vS.Distance(Spans[j].curve.End);
        //        }
        //    }
        //}
    }
}