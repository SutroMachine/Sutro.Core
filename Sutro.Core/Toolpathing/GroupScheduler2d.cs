using g3;
using Sutro.Core.FillTypes;
using Sutro.Core.Toolpaths;
using System;
using System.Collections.Generic;

namespace Sutro.Core.Toolpathing
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

        public IFillPathScheduler2d TargetScheduler;

        protected SortingScheduler2d CurrentSorter;

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
            EndGroup();
        }

        public virtual void BeginGroup()
        {
            if (CurrentSorter != null)
                throw new Exception("GroupScheduler.BeginGroup: already in a group!");

            CurrentSorter = new SortingScheduler2d();
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

        SpeedHint IFillPathScheduler2d.SpeedHint { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public virtual void AppendCurveSets(List<FillCurveSet2d> paths)
        {
            if (CurrentSorter == null)
            {
                TargetScheduler.AppendCurveSets(paths);
                throw new Exception("TODO: need to update lastPoint...");
            }
            else
            {
                CurrentSorter.SpeedHint = SpeedHint;
                CurrentSorter.AppendCurveSets(paths);
            }
        }
    }
}