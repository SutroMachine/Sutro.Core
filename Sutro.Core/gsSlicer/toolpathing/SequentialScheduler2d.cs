using g3;
using gs.FillTypes;
using Sutro.Core.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace gs
{
    // dumbest possible scheduler...
    public class SequentialScheduler2d : IFillPathScheduler2d
    {
        public ToolpathSetBuilder Builder;
        public IPrintProfileFFF Settings;

        public bool ExtrudeOnShortTravels = false;
        public double ShortTravelDistance = 0;

        public double LayerZ { get; }

        // optional function we will call when curve sets are appended
        public Action<List<FillCurveSet2d>, SequentialScheduler2d> OnAppendCurveSetsF = null;

        public SequentialScheduler2d(ToolpathSetBuilder builder, IPrintProfileFFF settings, double layerZ)
        {
            Builder = builder;
            Settings = settings;
            LayerZ = layerZ;
        }

        public virtual SpeedHint SpeedHint { get; set; } = SpeedHint.Default;

        public Vector2d CurrentPosition => Builder.Position.xy;

        public virtual void AppendCurveSets(List<FillCurveSet2d> fillSets)
        {
            OnAppendCurveSetsF?.Invoke(fillSets, this);
            foreach (var curveSet in fillSets)
            {
                foreach (var curve in curveSet.Curves)
                    AppendFillCurve(curve);

                foreach (var loop in curveSet.Loops)
                    AppendFillLoop(loop);
            }
        }

        // [TODO] no reason we couldn't start on edge midpoint??
        public virtual void AppendFillLoop(FillLoop loop)
        {
            AssertValidLoop(loop);

            var oriented = SelectLoopDirection(loop);
            var rolled = SelectLoopEntry(oriented, Builder.Position.xy);

            AppendTravel(Builder.Position.xy, rolled.Entry);

            double useSpeed = SelectSpeed(rolled);
            BuildLoop(rolled, useSpeed);
        }

        protected virtual FillLoop SelectLoopEntry(FillLoop loop, Vector2d currentPosition)
        {
            var location = FindLoopEntryPoint(loop, currentPosition);
            return loop.RollBetweenVertices(location);
        }

        protected virtual void BuildLoop(FillLoop loop, double useSpeed)
        {
            if (!(loop is FillLoop<FillSegment> o))
                throw new NotImplementedException($"FillPathScheduler2d does not support type {loop.GetType()}.");
            BuildLoopConcrete(o, useSpeed);
        }

        protected virtual void BuildLoopConcrete<TSegment>(FillLoop<TSegment> rolled, double useSpeed) where TSegment : IFillSegment, new()
        {
            Builder.AppendExtrude(rolled.Vertices(true).ToList(), useSpeed, rolled.FillType, null);
        }

        protected virtual FillLoop SelectLoopDirection(FillLoop loop)
        {
            if (loop.IsHoleShell != loop.IsClockwise())
                return loop.Reversed();
            return loop;
        }

        protected virtual ElementLocation FindLoopEntryPoint(FillLoop poly, Vector2d currentPos2)
        {
            int startIndex;
            if (Settings.Part.ZipperAlignedToPoint && poly.FillType.IsEntryLocationSpecified())
            {
                // split edges to position zipper closer to the desired point?
                // TODO: Enter midsegment
                Vector2d zipperLocation = new Vector2d(Settings.Part.ZipperLocationX, Settings.Part.ZipperLocationY);
                startIndex = CurveUtils2.FindNearestVertex(zipperLocation, poly.Vertices(true));
            }
            else if (Settings.Part.ShellRandomizeStart && poly.FillType.IsEntryLocationSpecified())
            {
                // split edges for a actual random location along the perimeter instead of a random vertex?
                Random rnd = new Random();
                startIndex = rnd.Next(poly.ElementCount);
            }
            else
            {
                // use the vertex closest to the current nozzle position
                startIndex = CurveUtils2.FindNearestVertex(currentPos2, poly.Vertices(true));
            }

            return new ElementLocation(startIndex, 0);
        }

        protected virtual void AppendTravel(Vector2d startPt, Vector2d endPt)
        {
            double travelDistance = startPt.Distance(endPt);

            // a travel may require a retract, which we might want to skip
            if (ExtrudeOnShortTravels &&
                travelDistance < ShortTravelDistance)
            {
                // TODO: Add strategy for extrude move?
                Builder.AppendExtrude(endPt, Settings.Part.RapidTravelSpeed, new DefaultFillType());
            }
            else if (Settings.Part.TravelLiftEnabled &&
                travelDistance > Settings.Part.TravelLiftDistanceThreshold)
            {
                Builder.AppendMoveToZ(LayerZ + Settings.Part.TravelLiftHeight, Settings.Part.ZTravelSpeed, ToolpathTypes.Travel);
                Builder.AppendTravel(endPt, Settings.Part.RapidTravelSpeed);
                Builder.AppendMoveToZ(LayerZ, Settings.Part.ZTravelSpeed, ToolpathTypes.Travel);
            }
            else
            {
                Builder.AppendTravel(endPt, Settings.Part.RapidTravelSpeed);
            }
        }

        // [TODO] would it ever make sense to break polyline to avoid huge travel??
        public virtual void AppendFillCurve(FillCurve curve)
        {
            Vector3d currentPos = Builder.Position;
            Vector2d currentPos2 = currentPos.xy;

            AssertValidCurve(curve);

            var oriented = OrientCurve(curve, currentPos2);

            AppendTravel(currentPos2, oriented.Entry);

            BuildCurve(oriented, SelectSpeed(oriented));
        }

        protected static FillCurve OrientCurve(FillCurve curve, Vector2d currentPos2)
        {
            if (curve.Entry.DistanceSquared(currentPos2) > curve.Exit.DistanceSquared(currentPos2))
            {
                return curve.Reversed();
            }

            return curve;
        }

        protected virtual void BuildCurve(FillCurve curve, double useSpeed)
        {
            if (!(curve is FillCurve<FillSegment> o))
                throw new NotImplementedException($"FillPathScheduler2d.BuildCurve does not support type {curve.GetType()}.");
            BuildCurveConcrete(o, useSpeed);
        }

        protected void BuildCurveConcrete<TSegment>(FillCurve<TSegment> curve, double useSpeed) where TSegment : IFillSegment, new()
        {
            var vertices = curve.Vertices().ToList();
            var flags = CreateToolpathVertexFlags(curve);
            var dimensions = GetFillDimensions(curve);
            Builder.AppendExtrude(vertices, useSpeed, dimensions, curve.FillType, curve.IsHoleShell, flags);
        }

        protected static Vector2d GetFillDimensions(FillBase curve)
        {
            Vector2d dimensions = GCodeUtil.UnspecifiedDimensions;
            if (curve.FillThickness > 0)
                dimensions.x = curve.FillThickness;
            return dimensions;
        }

        protected static List<TPVertexFlags> CreateToolpathVertexFlags<TSegment>(FillCurve<TSegment> curve)
            where TSegment : IFillSegment, new()
        {
            var flags = new List<TPVertexFlags>(curve.Elements.Count + 1);
            for (int i = 0; i < curve.Elements.Count + 1; i++)
            {
                var flag = TPVertexFlags.None;

                if (i == 0)
                    flag = TPVertexFlags.IsPathStart;
                else
                {
                    var segInfo = curve.Elements[i - 1].Edge;
                    if (segInfo.IsConnector)
                        flag = TPVertexFlags.IsConnector;
                }

                flags.Add(flag);
            }

            return flags;
        }

        // 1) If we have "careful" speed hint set, use CarefulExtrudeSpeed
        //       (currently this is only set on first layer)
        public virtual double SelectSpeed(FillBase pathCurve)
        {
            double speed = SpeedHint == SpeedHint.Careful ?
                Settings.Part.CarefulExtrudeSpeed : Settings.Part.RapidExtrudeSpeed;

            return pathCurve.FillType.ModifySpeed(speed, SpeedHint);
        }

        protected void AssertValidCurve(FillCurve curve)
        {
            int N = curve.ElementCount;
            if (N < 1)
            {
                StackFrame frame = new StackFrame(1);
                var method = frame.GetMethod();
                var type = method.DeclaringType;
                var name = method.Name;
                throw new ArgumentException($"{type}.{name}: degenerate curve; must have at least 1 edge.");
            }
        }

        protected void AssertValidLoop(FillLoop curve)
        {
            int N = curve.ElementCount;
            if (N < 2)
            {
                StackFrame frame = new StackFrame(1);
                var method = frame.GetMethod();
                var type = method.DeclaringType;
                var name = method.Name;
                throw new ArgumentException($"{type}.{name}: degenerate loop; must have at least 2 edges");
            }
        }
    }
}