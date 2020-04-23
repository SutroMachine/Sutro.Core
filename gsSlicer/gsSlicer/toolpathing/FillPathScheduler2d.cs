﻿using g3;
using gs.FillTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace gs
{
    public enum SchedulerSpeedHint
    {
        Careful, Default, Rapid, MaxSpeed
    }

    public interface IFillPathScheduler2d
    {
        void AppendCurveSets(List<FillCurveSet2d> paths);

        SchedulerSpeedHint SpeedHint { get; set; }

        Vector2d CurrentPosition { get; }

    }

    // dumbest possible scheduler...
    public class SequentialScheduler2d : IFillPathScheduler2d
    {
        public ToolpathSetBuilder Builder;
        public SingleMaterialFFFSettings Settings;

        public bool ExtrudeOnShortTravels = false;
        public double ShortTravelDistance = 0;

        // optional function we will call when curve sets are appended
        public Action<List<FillCurveSet2d>, SequentialScheduler2d> OnAppendCurveSetsF = null;

        public SequentialScheduler2d(ToolpathSetBuilder builder, SingleMaterialFFFSettings settings)
        {
            Builder = builder;
            Settings = settings;
        }

        private SchedulerSpeedHint speed_hint = SchedulerSpeedHint.Default;

        public virtual SchedulerSpeedHint SpeedHint
        {
            get { return speed_hint; }
            set { speed_hint = value; }
        }

        public Vector2d CurrentPosition => Builder.Position.xy;

        protected virtual void AppendFill(FillElementBase<BasicSegmentInfo> fill)
        {
            switch (fill)
            {
                case BasicFillLoop basicFillLoop:
                    AppendBasicFillLoop(basicFillLoop);
                    break;
                case BasicFillCurve basicFillCurve:
                    AppendBasicFillCurve(basicFillCurve);
                    break;
                default:
                    throw new NotImplementedException($"{fill.GetType()} not supported by {nameof(SequentialScheduler2d)}");
            }
        }

        public virtual void AppendCurveSets(List<FillCurveSet2d> fillSets)
        {
            OnAppendCurveSetsF?.Invoke(fillSets, this);
            foreach (var fill in FlattenFillCurveSets(fillSets))
            {
                AppendFill(fill);
            }
        }

        protected static List<FillElementBase<BasicSegmentInfo>> FlattenFillCurveSets(List<FillCurveSet2d> fillSets)
        {
            var fillElements = new List<FillElementBase<BasicSegmentInfo>>();

            foreach (var fills in fillSets)
            {
                fillElements.AddRange(fills.Loops);
                fillElements.AddRange(fills.Curves);
            }

            return fillElements;
        }

        // [TODO] no reason we couldn't start on edge midpoint??
        public virtual void AppendBasicFillLoop(BasicFillLoop poly)
        {
            Vector3d currentPos = Builder.Position;
            Vector2d currentPos2 = currentPos.xy;

            AssertValidLoop(poly);

            int startIndex = FindLoopEntryPoint(poly, currentPos2);

            Vector2d startPt = poly[startIndex];

            AppendTravel(currentPos2, startPt);

            List<Vector2d> loopV = new List<Vector2d>(poly.VertexCount + 1);
            for (int i = 0; i <= poly.VertexCount; i++)
            {
                int k = (startIndex + i) % poly.VertexCount;
                loopV.Add(poly[k]);
            }

            double useSpeed = SelectSpeed(poly);

            Builder.AppendExtrude(loopV, useSpeed, poly.FillType, null);
        }

        private int FindLoopEntryPoint(FillLoopBase<BasicSegmentInfo> poly, Vector2d currentPos2)
        {
            int startIndex;
            if (Settings.ZipperAlignedToPoint && poly.FillType.IsEntryLocationSpecified())
            {
                // split edges to position zipper closer to the desired point?
                Vector2d zipperLocation = new Vector2d(Settings.ZipperLocationX, Settings.ZipperLocationY);
                startIndex = CurveUtils2.FindNearestVertex(zipperLocation, poly.Vertices);
            }
            else if (Settings.ShellRandomizeStart && poly.FillType.IsEntryLocationSpecified())
            {
                // split edges for a actual random location along the perimeter instead of a random vertex?
                Random rnd = new Random();
                startIndex = rnd.Next(poly.VertexCount);
            }
            else
            {
                // use the vertex closest to the current nozzle position
                startIndex = CurveUtils2.FindNearestVertex(currentPos2, poly.Vertices);
            }

            return startIndex;
        }

        protected void AppendTravel(Vector2d startPt, Vector2d endPt)
        {
            double travelDistance = startPt.Distance(endPt);

            // a travel may require a retract, which we might want to skip
            if (ExtrudeOnShortTravels &&
                travelDistance < ShortTravelDistance)
            {
                // TODO: Add strategy for extrude move?
                Builder.AppendExtrude(endPt, Settings.RapidTravelSpeed, new DefaultFillType());
            }
            else if (Settings.TravelLiftEnabled &&
                travelDistance > Settings.TravelLiftDistanceThreshold)
            {
                Builder.AppendZChange(Settings.TravelLiftHeight, Settings.ZTravelSpeed, ToolpathTypes.Travel);
                Builder.AppendTravel(endPt, Settings.RapidTravelSpeed);
                Builder.AppendZChange(-Settings.TravelLiftHeight, Settings.ZTravelSpeed, ToolpathTypes.Travel);
            }
            else
            {
                Builder.AppendTravel(endPt, Settings.RapidTravelSpeed);
            }
        }

        // [TODO] would it ever make sense to break polyline to avoid huge travel??
        public virtual void AppendBasicFillCurve(FillCurveBase<BasicSegmentInfo> curve)
        {
            Vector3d currentPos = Builder.Position;
            Vector2d currentPos2 = currentPos.xy;

            AssertValidCurve(curve);

            if (curve.Start.DistanceSquared(currentPos2) > curve.End.DistanceSquared(currentPos2))
            {
                // This modifies input; avoid?
                curve.Reverse();
            }

            AppendTravel(currentPos2, curve[0]);

            var flags = new List<TPVertexFlags>(curve.VertexCount);

            var vertices = new List<Vector2d>(curve.VertexCount);
            for (int i = 0; i < curve.VertexCount; i++)
            {
                var flag = TPVertexFlags.None;

                if (i == 0)
                    flag = TPVertexFlags.IsPathStart;
                else
                {
                    var segInfo = curve.GetSegmentDataBeforeVertex(i);
                    if (segInfo != null && segInfo.IsConnector)
                        flag = TPVertexFlags.IsConnector;
                }

                vertices.Add(curve[i]);
                flags.Add(flag);
            }

            double useSpeed = SelectSpeed(curve);

            Vector2d dimensions = GCodeUtil.UnspecifiedDimensions;
            if (curve.CustomThickness > 0)
                dimensions.x = curve.CustomThickness;
            Builder.AppendExtrude(vertices, useSpeed, dimensions, curve.FillType, curve.IsHoleShell, flags);
        }

        // 1) If we have "careful" speed hint set, use CarefulExtrudeSpeed
        //       (currently this is only set on first layer)
        public virtual double SelectSpeed(FillElementBase<BasicSegmentInfo> pathCurve)
        {
            double speed = SpeedHint == SchedulerSpeedHint.Careful ?
                Settings.CarefulExtrudeSpeed : Settings.RapidExtrudeSpeed;

            return pathCurve.FillType.ModifySpeed(speed, SpeedHint);
        }

        protected void AssertValidCurve(FillCurveBase<BasicSegmentInfo> curve)
        {
            int N = curve.VertexCount;
            if (N < 2)
            {
                StackFrame frame = new StackFrame(1);
                var method = frame.GetMethod();
                var type = method.DeclaringType;
                var name = method.Name;
                throw new ArgumentException($"{type}.{name}: degenerate curve; must have at least 2 points.");
            }
        }

        protected void AssertValidLoop(FillLoopBase<BasicSegmentInfo> curve)
        {
            int N = curve.VertexCount;
            if (N < 3)
            {
                StackFrame frame = new StackFrame(1);
                var method = frame.GetMethod();
                var type = method.DeclaringType;
                var name = method.Name;
                throw new ArgumentException($"{type}.{name}: degenerate loop; must have at least 3 points");
            }
        }
    }
}