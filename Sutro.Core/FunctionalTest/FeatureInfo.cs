using g3;
using System.Collections.Generic;

namespace Sutro.Core.FunctionalTest
{
    public class FeatureInfo : IFeatureInfo
    {
        public AxisAlignedBox2d BoundingBox;

        public Vector2d UnweightedCenterOfMass;

        public Vector2d CenterOfMass => UnweightedCenterOfMass / Extrusion;

        public double Distance { get; set; }

        public double Duration { get; set; }

        public double Extrusion { get; set; }

        public string FillType { get; set; }

        protected double BoundingBoxTolerance { get; } = 1e-1;
        protected double CenterOfMassTolerance { get; } = 1e-1;
        protected double DistanceTolerance { get; } = 1e-1;
        protected double DurationTolerance { get; } = 1e-1;
        protected double ExtrusionTolerance { get; } = 1e-1;

        public FeatureInfo()
        {
            BoundingBox = new AxisAlignedBox2d(false);
        }

        public FeatureInfo(string fillType) : this()
        {
            FillType = fillType;
        }

        public virtual void Add(IFeatureInfo other)
        {
            Add((FeatureInfo)other);
        }

        public virtual void Add(FeatureInfo other)
        {
            BoundingBox.Contain(other.BoundingBox);
            Distance += other.Distance;
            Duration += other.Duration;
            Extrusion += other.Extrusion;
            UnweightedCenterOfMass += other.UnweightedCenterOfMass;
        }

        public virtual IEnumerable<Comparison> Compare(IFeatureInfo other)
        {
            return Compare((FeatureInfo)other);
        }

        public virtual IEnumerable<Comparison> Compare(FeatureInfo expected)
        {
            yield return CompareBoundingBoxes(expected.BoundingBox);
            yield return CompareCenterOfMass(expected.CenterOfMass);
            yield return CompareDeposition(expected.Extrusion);
            yield return CompareDistance(expected.Distance);
            yield return CompareDuration(expected.Duration);
        }

        protected static string GetComparisonString(string label, double expected, double actual, string fmt)
        {
            double differenceAbs = actual - expected;
            double differencePct = expected == 0 ? double.PositiveInfinity : differenceAbs / expected;
            string arrow = differenceAbs > 0 ? "▲" : "▼";
            return $"{label}: expected {expected.ToString(fmt)}, actual {actual.ToString(fmt)}. {arrow} {differenceAbs.ToString(fmt)} ({differencePct:P1})";
        }

        protected virtual Comparison CompareBoundingBoxes(AxisAlignedBox2d expected)
        {
            if (!BoundingBox.Equals(expected, BoundingBoxTolerance))
                return new Comparison(false, $"bounding boxes: expected {expected}, actual {BoundingBox}");

            return new Comparison(true, $"bounding boxes: {expected}");
        }

        protected virtual Comparison CompareCenterOfMass(Vector2d expected)
        {
            if (!CenterOfMass.EpsilonEqual(expected, CenterOfMassTolerance))
                return new Comparison(false, $"center of mass: expected {expected}, actual {CenterOfMass}");

            return new Comparison(true, $"center of mass: {expected}");
        }

        protected virtual Comparison CompareDeposition(double expected)
        {
            if (!MathUtil.EpsilonEqual(Extrusion, expected, ExtrusionTolerance))
            {
                return new Comparison(false, GetComparisonString("deposition", expected, Extrusion, "F3"));
            }

            return new Comparison(true, $"deposition: {expected:F3}");
        }

        protected virtual Comparison CompareDuration(double expected)
        {
            if (!MathUtil.EpsilonEqual(Duration, expected, DurationTolerance))
                return new Comparison(false, GetComparisonString("duration", expected, Duration, "F3"));

            return new Comparison(true, $"duration: {expected:F1}");
        }

        private Comparison CompareDistance(double expected)
        {
            if (!MathUtil.EpsilonEqual(Distance, expected, DistanceTolerance))
                return new Comparison(false, GetComparisonString("distance", expected, Distance, "F3"));

            return new Comparison(true, $"distance: {expected:F3}");
        }
    }
}