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
    public class SortingScheduler2d : ISortingScheduler2d
    {
        private readonly FillEntryPicker entryPicker;

        public SortingScheduler2d(FillEntryPicker entryPicker, Vector2d startPoint)
        {
            this.entryPicker = entryPicker;
            CurrentPosition = startPoint;
        }

        public SpeedHint SpeedHint { get; set; }

        /// <summary>
        /// Final point in the output paths, computed by SortAndAppendTo()
        /// </summary>
        public Vector2d CurrentPosition { get; private set; }

        protected List<FillCurveSet2d> fillSets = new List<FillCurveSet2d>();

        public virtual void AppendCurveSets(List<FillCurveSet2d> fillSets)
        {
            this.fillSets.AddRange(fillSets);
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
            var remaining = new HashSet<FillEntryPicker.SolverBase>();
            foreach (var fillSet in fillSets)
            {
                foreach (var loop in fillSet.Loops)
                    remaining.Add(entryPicker.CreateSolver(loop));
                foreach (var curve in fillSet.Curves)
                    remaining.Add(entryPicker.CreateSolver(curve));
            }

            var orientedFills = new List<FillBase>();
            while (remaining.Count > 0)
            {
                // Find nearest 
                (FillEntryPicker.SolverBase Solver, FillEntryPicker.SolutionBase Solution) closest = (null, null);
                foreach (var solver in remaining)
                {
                    var solution = solver.OrientToPoint(startPoint);
                    if (closest.Solution == null || solution.Distance < closest.Solution.Distance)
                    {
                        closest = (solver, solution);
                    }
                }

                // Add to 
                remaining.Remove(closest.Solver);
                var fill = closest.Solution.GetSolution();
                orientedFills.Add(fill);
                startPoint = fill.Exit;
            }

            return orientedFills;
        }
    }
}