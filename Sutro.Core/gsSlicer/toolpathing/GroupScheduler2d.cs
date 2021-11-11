using g3;
using gs.FillTypes;
using System;
using System.Collections.Generic;

namespace gs
{
    /// <summary>
    /// GroupScheduler collects up paths and then optimizes their scheduling.
    /// You call BeginGroup() / EndGroup() around semantic groups of paths.
    /// Inside each group, a SortingScheduler is used to re-order the paths.
    /// These oredered paths are then passed to an input IPathScheduler on EndGroup()
    /// </summary>
    public class GroupScheduler2d : IFillPathScheduler2d
    {
        public SpeedHint SpeedHint
        {
            get { return TargetScheduler.SpeedHint; }
            set { TargetScheduler.SpeedHint = value; }
        }

        public IFillPathScheduler2d TargetScheduler { get; }

        public Func<ISortingScheduler2d> SorterFactory { get; set; } = () => new SortingScheduler2d();

        protected ISortingScheduler2d CurrentSorter;

        protected Vector2d lastPoint;

        public Vector2d CurrentPosition
        {
            get { return lastPoint; }
        }

        public GroupScheduler2d(IFillPathScheduler2d target, Vector2d startPoint)
        {
            TargetScheduler = target;
            lastPoint = startPoint;
        }

        ~GroupScheduler2d()
        {
            if (CurrentSorter != null)
                EndGroup();
        }

        public virtual void BeginGroup()
        {
            if (CurrentSorter != null)
                throw new InvalidOperationException("Cannot begin a new group before ending the current group!");

            CurrentSorter = SorterFactory();
        }

        public virtual void EndGroup()
        {
            if (CurrentSorter != null)
            {
                CurrentSorter.SortAndAppendTo(lastPoint, TargetScheduler);
                lastPoint = CurrentSorter.CurrentPosition;
                CurrentSorter = null;
            }
        }

        public virtual bool InGroup
        {
            get { return CurrentSorter != null; }
        }

        public virtual void AppendCurveSets(List<FillCurveSet2d> paths)
        {
            if (CurrentSorter == null)
            {
                throw new InvalidOperationException("Cannot append curves before starting a new group!");
            }
            CurrentSorter.SpeedHint = this.SpeedHint;
            CurrentSorter.AppendCurveSets(paths);
        }
    }
}