using g3;
using gs.FillTypes;
using Sutro.Core.Settings;
using System;
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
    public class SortingScheduler2d : ISortingScheduler2d
    {
        protected abstract class SolutionBase
        {
            public double Distance { get; protected set; }
            public abstract FillBase GetSolution();

        }

        protected class LoopSolution : SolutionBase
        {
            private readonly Func<FillLoop> func;

            public LoopSolution(double distance, Func<FillLoop> func)
            {
                Distance = distance;
                this.func = func;
            }

            public override FillBase GetSolution() { return func(); }
        }

        protected class CurveSolution : SolutionBase
        {
            private readonly Func<FillCurve> func;
            public CurveSolution(double distance, Func<FillCurve> func)
            {
                Distance = distance;
                this.func = func;
            }

            public override FillBase GetSolution() { return func(); }
        }
       
        protected abstract class SolverBase
        {
            public abstract SolutionBase OrientToPoint(Vector2d point);
        }

        protected class SolverLoopRandomEntryVertex : SolverBase
        {
            private readonly int startIndex;
            private readonly Vector2d seam;
            private readonly FillLoop loop;

            public SolverLoopRandomEntryVertex(FillLoop loop)
            {
                this.loop = loop;
                startIndex = new Random().Next(loop.ElementCount);
                seam = loop.GetVertex(startIndex).xy;
            }

            private FillLoop RollLoopToVertex()
            {
                // TODO: Add reverse for holes
                return loop.RollToVertex(startIndex);
            }

            public override SolutionBase OrientToPoint(Vector2d point)
            {
                return new LoopSolution(point.Distance(seam), RollLoopToVertex);
            }
        }

        protected class SolverLoopZipperEntryVertex : SolverBase
        {
            private readonly int startIndex;
            private readonly Vector2d seam;
            private readonly FillLoop loop;

            public SolverLoopZipperEntryVertex(FillLoop loop, Vector2d zipperLocation)
            {
                this.loop = loop;
                startIndex = CurveUtils2.FindNearestVertex(zipperLocation, loop.Vertices(true));
                seam = loop.GetVertex(startIndex).xy;
            }

            private FillLoop RollLoopToVertex()
            {
                // TODO: Add reverse for holes
                return loop.RollToVertex(startIndex);
            }

            public override SolutionBase OrientToPoint(Vector2d point)
            {
                return new LoopSolution(point.Distance(seam), RollLoopToVertex);
            }
        }

        protected class SolverLoopClosestVertex : SolverBase
        {
            private readonly FillLoop loop;

            public SolverLoopClosestVertex(FillLoop loop)
            {
                this.loop = loop;
            }

            private FillLoop RollLoopToVertex(int startIndex)
            {
                // TODO: Add reverse for holes
                return loop.RollToVertex(startIndex);
            }

            public override SolutionBase OrientToPoint(Vector2d point)
            {
                int startIndex = CurveUtils2.FindNearestVertex(point, loop.Vertices(true));
                Vector2d startPoint = loop.GetVertex(startIndex).xy;
                return new LoopSolution(point.Distance(startPoint), () => RollLoopToVertex(startIndex));
            }
        }

        protected class SolverCurveFixedOrientation : SolverBase
        {
            private readonly FillCurve curve;

            public SolverCurveFixedOrientation(FillCurve curve)
            {
                this.curve = curve;
            }

            public override SolutionBase OrientToPoint(Vector2d point)
            {
                double dStart = curve.Entry.Distance(point);
                return new CurveSolution(dStart, () => curve);
            }
        }

        protected class SolverCurveClosestVertex : SolverBase
        {
            private readonly FillCurve curve;

            public SolverCurveClosestVertex(FillCurve curve)
            {
                this.curve = curve;
            }

            private FillCurve Orient(bool reverse)
            {
                return reverse ? curve.Reversed() : curve;
            }

            public override SolutionBase OrientToPoint(Vector2d point)
            {

                double dStart = curve.Entry.Distance(point);
                double dEnd = curve.Exit.Distance(point);
                bool reverse = dStart > dEnd;

                return new CurveSolution(Math.Min(dStart, dEnd), () => Orient(reverse));
            }
        }

        public IPrintProfileFFF Settings { get; }

        public SortingScheduler2d(IPrintProfileFFF settings)
        {
            Settings = settings;
        }

        public SpeedHint SpeedHint { get; set; }

        /// <summary>
        /// Final point in the output paths, computed by SortAndAppendTo()
        /// </summary>
        public Vector2d CurrentPosition { get; private set; }

        protected List<FillCurveSet2d> fillSets;

        public virtual void AppendCurveSets(List<FillCurveSet2d> fillSets)
        {
            this.fillSets = fillSets;
        }

        public virtual void SortAndAppendTo(Vector2d startPoint, IFillPathScheduler2d targetScheduler)
        {
            if (fillSets == null || fillSets.Count == 0)
                return;

            var sorted = FindShortestPath(startPoint);
            foreach (var fill in sorted)
            {
                var curveSet = new FillCurveSet2d();
                curveSet.Append(fill);
                targetScheduler.AppendCurveSets(new List<FillCurveSet2d>() { curveSet });
            }
        
            CurrentPosition = targetScheduler.CurrentPosition;
            fillSets = new List<FillCurveSet2d>();
        }

        protected virtual List<FillBase> FindShortestPath(Vector2d startPoint)
        {
            // Set up set of fills to be sorted
            var remaining = new HashSet<SolverBase>();
            foreach (var fillSet in fillSets)
            {
                foreach (var loop in fillSet.Loops)
                    remaining.Add(CreateSolver(loop));
                foreach (var curve in fillSet.Curves)
                    remaining.Add(CreateSolver(curve));
            }

            var orientedFills = new List<FillBase>();
            while (remaining.Count > 0)
            {
                // Find nearest 
                SolutionBase closestSolution = null;
                SolverBase closestSolver = null;
                foreach (var solver in remaining)
                {
                    var solution = solver.OrientToPoint(startPoint);
                    if (closestSolution == null || solution.Distance < closestSolution.Distance)
                    {
                        closestSolution = solution;
                        closestSolver = solver;
                    }
                }

                // Add to 
                remaining.Remove(closestSolver);
                var fill = closestSolution.GetSolution();
                orientedFills.Add(fill);
                startPoint = fill.Exit;
            }

            return orientedFills;
        }

        private SolverBase CreateSolver(FillLoop loop)
        {
            if (loop.FillType.IsEntryLocationSpecified())
            {
                if (Settings.Part.ZipperAlignedToPoint)
                {
                    return new SolverLoopZipperEntryVertex(loop, Settings.Part.ZipperLocation());
                }
                if (Settings.Part.ShellRandomizeStart)
                {
                    return new SolverLoopRandomEntryVertex(loop);
                }
            }

            return new SolverLoopClosestVertex(loop);
        }

        private SolverBase CreateSolver(FillCurve curve)
        {
            if (curve.FillType.IsPartShell())
                return new SolverCurveFixedOrientation(curve);

            return new SolverCurveClosestVertex(curve);
        }

        protected virtual FillLoop SelectLoopDirection(FillLoop loop)
        {
            if (loop.IsHoleShell != loop.IsClockwise())
                return loop.Reversed();
            return loop;
        }
    }
}