using g3;
using gs.FillTypes;
using Sutro.Core.Settings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace gs
{

    /// <summary> Simplest possible scheduler; does not change fill orientation or entry locations</summary>
    /// <remarks>
    /// Will simply schedule the fills in the order they are passed in, without applying 
    /// any modifications like rolling loops or orienting curves. Contains the base logic for converting
    /// fills to toolpaths, and can be used as the target for more advanced schedulers.
    /// </remarks>
    public class SequentialScheduler2d : IFillPathScheduler2d
    {
        public ToolpathSetBuilder Builder { get; }
        public IPrintProfileFFF Settings { get; }

        public bool ExtrudeOnShortTravels { get; set; } = false;
        public double ShortTravelDistance { get; set; } = 0;
        public double LayerZ { get; }

        // Optional function we will call when curve sets are appended
        public Action<List<FillCurveSet2d>, SequentialScheduler2d> OnAppendCurveSetsF { get; set; } = null;

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

        public virtual void AppendFillLoop(FillLoop loop)
        {
            AssertValidLoop(loop);
            AppendTravel(Builder.Position.xy, loop.Entry);
            BuildLoop(loop, SelectSpeed(loop));
        }

        protected virtual void BuildLoop(FillLoop loop, double useSpeed)
        {
            if (!(loop is FillLoop<FillSegment> o))
                throw new NotImplementedException($"SequentialScheduler2d does not support type {loop.GetType()}.");

            BuildLoopConcrete(o, useSpeed);
        }

        protected virtual void BuildLoopConcrete<TSegment>(FillLoop<TSegment> rolled, double useSpeed)
            where TSegment : IFillSegment, new()
        {
            Builder.AppendExtrude(rolled.Vertices(true).ToList(), useSpeed, rolled.FillType, null);
        }

        protected virtual void AppendTravel(Vector2d startPt, Vector2d endPt)
        {
            double travelDistance = startPt.Distance(endPt);

            // a travel may require a retract, which we might want to skip
            if (ExtrudeOnShortTravels && travelDistance < ShortTravelDistance)
            {
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

        public virtual void AppendFillCurve(FillCurve curve)
        {
            Vector3d currentPos = Builder.Position;
            Vector2d currentPos2 = currentPos.xy;

            AssertValidCurve(curve);
            AppendTravel(currentPos2, curve.Entry);
            BuildCurve(curve, SelectSpeed(curve));
        }

        protected virtual void BuildCurve(FillCurve curve, double useSpeed)
        {
            if (!(curve is FillCurve<FillSegment> o))
                throw new NotImplementedException($"FillPathScheduler2d.BuildCurve does not support type {curve.GetType()}.");
            BuildCurveConcrete(o, useSpeed);
        }

        protected void BuildCurveConcrete<TSegment>(FillCurve<TSegment> curve, double useSpeed)
            where TSegment : IFillSegment, new()
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
                {
                    flag = TPVertexFlags.IsPathStart;
                }
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