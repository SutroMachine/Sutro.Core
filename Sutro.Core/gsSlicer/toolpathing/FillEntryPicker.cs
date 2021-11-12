using g3;
using Sutro.Core.Settings;
using System;

namespace gs
{
    public class FillEntryPicker
    {
        public FillEntryPicker(IPrintProfileFFF settings)
        {
            this.settings = settings;
        }

        protected readonly IPrintProfileFFF settings;

        public virtual SolverBase CreateSolver(FillLoop loop)
        {
            if (loop.FillType.IsEntryLocationSpecified())
            {
                if (settings.Part.ZipperAlignedToPoint)
                {
                    return new SolverLoopZipperEntryVertex(loop, settings.Part.ZipperLocation());
                }
                if (settings.Part.ShellRandomizeStart)
                {
                    return new SolverLoopRandomEntryVertex(loop);
                }
            }

            return new SolverLoopClosestVertex(loop);
        }

        public virtual SolverBase CreateSolver(FillCurve curve)
        {
            if (curve.FillType.IsPartShell())
                return new SolverCurveFixedOrientation(curve);

            return new SolverCurveClosestVertex(curve);
        }

        public abstract class SolutionBase
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

            public override FillBase GetSolution()
            { return func(); }
        }

        protected class CurveSolution : SolutionBase
        {
            private readonly Func<FillCurve> func;

            public CurveSolution(double distance, Func<FillCurve> func)
            {
                Distance = distance;
                this.func = func;
            }

            public override FillBase GetSolution()
            { return func(); }
        }

        public abstract class SolverBase
        {
            public abstract SolutionBase OrientToPoint(Vector2d point);
        }

        protected abstract class SolverLoopBase : SolverBase
        {
            protected readonly FillLoop loop;

            protected virtual FillLoop OrientLoop(FillLoop loop)
            {
                if (loop.IsHoleShell != loop.IsClockwise())
                    return loop.Reversed();
                return loop;
            }

            protected SolverLoopBase(FillLoop loop)
            {
                this.loop = loop;
            }
        }

        protected class SolverLoopRandomEntryVertex : SolverLoopBase
        {
            private readonly int startIndex;
            private readonly Vector2d seam;

            public SolverLoopRandomEntryVertex(FillLoop loop) : base(loop)
            {
                startIndex = new Random().Next(loop.ElementCount);
                seam = loop.GetVertex(startIndex).xy;
            }

            private FillLoop RollLoopToVertex()
            {
                return OrientLoop(loop.RollToVertex(startIndex));
            }

            public override SolutionBase OrientToPoint(Vector2d point)
            {
                return new LoopSolution(point.Distance(seam), RollLoopToVertex);
            }
        }

        protected class SolverLoopZipperEntryVertex : SolverLoopBase
        {
            private readonly int startIndex;
            private readonly Vector2d seam;

            public SolverLoopZipperEntryVertex(FillLoop loop, Vector2d zipperLocation) : base(loop)
            {
                startIndex = CurveUtils2.FindNearestVertex(zipperLocation, loop.Vertices(true));
                seam = loop.GetVertex(startIndex).xy;
            }

            private FillLoop RollLoopToVertex()
            {
                return OrientLoop(loop.RollToVertex(startIndex));
            }

            public override SolutionBase OrientToPoint(Vector2d point)
            {
                return new LoopSolution(point.Distance(seam), RollLoopToVertex);
            }
        }

        protected class SolverLoopClosestVertex : SolverLoopBase
        {
            public SolverLoopClosestVertex(FillLoop loop) : base(loop)
            {
            }

            private FillLoop RollLoopToVertex(int startIndex)
            {
                return OrientLoop(loop.RollToVertex(startIndex));
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
    }
}