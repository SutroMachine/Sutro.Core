using g3;
using gs.FillTypes;
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
        public SpeedHint SpeedHint { get; set; }

        /// <summary>
        /// Final point in the output paths, computed by SortAndAppendTo()
        /// </summary>
        public Vector2d CurrentPosition { get; private set; }

        protected class PathItem
        {
            public SpeedHint speedHint;
        }

        protected class PathLoop : PathItem
        {
            public FillLoop loop;
            public bool reverse = false;
        }

        protected List<PathLoop> Loops = new List<PathLoop>();

        protected class PathSpan : PathItem
        {
            public FillCurve curve;
            public bool reverse = false;
        }

        protected List<PathSpan> Spans = new List<PathSpan>();

        public virtual void AppendCurveSets(List<FillCurveSet2d> paths)
        {
            foreach (FillCurveSet2d polySet in paths)
            {
                foreach (var loop in polySet.Loops)
                    Loops.Add(new PathLoop() { loop = loop, speedHint = SpeedHint });

                foreach (var curve in polySet.Curves)
                    Spans.Add(new PathSpan() { curve = curve, speedHint = SpeedHint });
            }
        }

        public virtual void SortAndAppendTo(Vector2d startPoint, IFillPathScheduler2d scheduler)
        {
            var saveHint = scheduler.SpeedHint;
            CurrentPosition = startPoint;

            List<Index3i> sorted = find_short_path_v1(startPoint);
            foreach (Index3i idx in sorted)
            {
                FillCurveSet2d paths = new FillCurveSet2d();

                SpeedHint pathHint = SpeedHint.Default;
                if (idx.a == 0)
                { // loop
                    PathLoop loop = Loops[idx.b];
                    pathHint = loop.speedHint;
                    if (idx.c != 0)
                    {
                        var rolled = loop.loop.RollToVertex(idx.c);
                        paths.Append(rolled);
                        CurrentPosition = rolled.Entry;
                    }
                    else
                    {
                        paths.Append(loop.loop);
                        CurrentPosition = loop.loop.Entry;
                    }
                }
                else
                {  // span
                    PathSpan span = Spans[idx.b];
                    if (idx.c == 1)
                        span.curve = span.curve.Reversed();
                    paths.Append(span.curve);
                    CurrentPosition = span.curve.Exit;
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
            return find_nearest(get_point(from), remaining);
        }

        protected virtual Index3i find_nearest(Vector2d pt, HashSet<Index2i> remaining)
        {
            double nearest = double.MaxValue;
            Index3i nearest_idx = Index3i.Max;
            foreach (Index2i idx in remaining)
            {
                if (idx.a == 0) // loop
                {
                    PathLoop loop = Loops[idx.b];
                    double distance = GetLoopEntryPoint(pt, loop, out var location);

                    if (distance < nearest)
                    {
                        nearest = distance;
                        nearest_idx = new Index3i(idx.a, idx.b, location.Index);
                    }
                }
                else // span
                {
                    PathSpan span = Spans[idx.b];
                    double distance = GetSpanEntryPoint(pt, span, out bool flip);

                    if (distance < nearest)
                    {
                        nearest = distance;
                        nearest_idx = new Index3i(idx.a, idx.b, flip ? 1 : 0);
                    }
                }
            }

            return nearest_idx;
        }

        protected virtual double GetSpanEntryPoint(Vector2d pt, PathSpan span, out bool flip)
        {
            double distanceToCurveStart = span.curve.Entry.Distance(pt);

            if (span.curve.FillType.IsEntryLocationSpecified())
            {
                flip = false;
                return distanceToCurveStart;
            }

            double distanceToCurveEnd = span.curve.Exit.Distance(pt);

            if (distanceToCurveStart < distanceToCurveEnd)
            {
                flip = false;
                return distanceToCurveStart;
            }
            else
            {
                flip = true;
                return distanceToCurveEnd;
            }
        }

        protected virtual double GetLoopEntryPoint(Vector2d startPoint, PathLoop loop,
            out ElementLocation location)
        {
            if (loop.loop.FillType.IsEntryLocationSpecified())
            {
                location = new ElementLocation(0, 0);
                return loop.loop.Entry.Distance(startPoint);
            }

            return loop.loop.FindClosestElementToPoint(startPoint, out location);
        }

        protected virtual Vector2d get_point(Index3i idx)
        {
            if (idx.a == 0)
            { // loop
                PathLoop loop = Loops[idx.b];
                return loop.loop.GetSegment2d(idx.c).Center;
            }
            else
            {  // span
                PathSpan span = Spans[idx.b];

                // [GDM] Reversed this logic 2019.10.23; by my thinking:
                // - if the curve ISN'T reversed, the exit point should be the end
                // - if the curve IS reversed, the exit point should be the start
                return (idx.c == 0) ? span.curve.Entry : span.curve.Exit;
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