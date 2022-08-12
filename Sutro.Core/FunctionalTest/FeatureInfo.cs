using g3;
using System.Collections.Generic;

namespace Sutro.Core.FunctionalTest
{
    public class FeatureInfo : IFeatureInfo
    {
        public FeatureInfo()
        {
        }

        public FeatureInfo(string fillType)
        {
            FillType = fillType;
        }

        public string FillType { get; set; }

        public AxisAlignedBox2d BoundingBox { get; } = AxisAlignedBox2d.Empty;
        public Vector2d CenterOfMass => UnweightedCenterOfMass / Extrusion;
        public Vector2d UnweightedCenterOfMass { get; set; }
        public double Extrusion { get; set; }
        public double Distance { get; set; }
        public double Duration { get; set; }

        protected double boundingBoxTolerance { get; } = 1e-1;
        protected double centerOfMassTolerance { get; } = 1e-1;
        protected double extrusionTolerance { get; } = 1e-1;
        protected double distanceTolerance { get; } = 1e-1;
        protected double durationTolerance { get; } = 1e-1;

        public override string ToString()
        {
            return
                "Bounding Box:\t" + BoundingBox +
                "\r\nCenter Of Mass:\t" + CenterOfMass +
                "\r\nExtrusion Amt:\t" + Extrusion +
                "\r\nExtrusion Dist:\t" + Distance +
                "\r\nExtrusion Time:\t" + Duration;
        }

        public virtual void Add(IFeatureInfo other)
        {
            Add((FeatureInfo)other);
        }

        public virtual void Add(FeatureInfo other)
        {
            BoundingBox.Contain(other.BoundingBox);
            Extrusion += other.Extrusion;
            Duration += other.Duration;
            Distance += other.Distance;
            UnweightedCenterOfMass += other.UnweightedCenterOfMass;
        }

        public virtual IEnumerable<string> Compare(IFeatureInfo other)
        {
            return Compare((FeatureInfo)other);
        }

        public virtual IEnumerable<string> Compare(FeatureInfo expected)
        {
            if (!BoundingBox.Equals(expected.BoundingBox, boundingBoxTolerance))
                yield return $"Bounding boxes aren't equal; expected {expected.BoundingBox}, got {BoundingBox}";

            if (!MathUtil.EpsilonEqual(Extrusion, expected.Extrusion, extrusionTolerance))
                yield return $"Cumulative extrusion amounts aren't equal; expected {expected.Extrusion}, got {Extrusion}";

            if (!MathUtil.EpsilonEqual(Duration, expected.Duration, durationTolerance))
                yield return $"Cumulative durations aren't equal; expected {expected.Duration}, got {Duration}";

            if (!MathUtil.EpsilonEqual(Distance, expected.Distance, distanceTolerance))
                yield return $"Cumulative distances aren't equal; expected {expected.Distance}, got {Distance}";

            if (!CenterOfMass.EpsilonEqual(expected.CenterOfMass, centerOfMassTolerance))
                yield return $"Centers of mass aren't equal; expected {expected.CenterOfMass}, got {CenterOfMass}";
        }
    }
}