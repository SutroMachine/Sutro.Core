﻿using g3;

namespace Sutro.Core.FunctionalTest
{
    public class FeatureInfo : IFeatureInfo
    {
        public FeatureInfo()
        {
        }

        public string FillType { get; set; }

        public AxisAlignedBox2d BoundingBox = AxisAlignedBox2d.Empty;
        public Vector2d CenterOfMass => UnweightedCenterOfMass / Extrusion;
        public Vector2d UnweightedCenterOfMass { get; set; }
        public double Extrusion { get; set; }
        public double Distance { get; set; }
        public double Duration { get; set; }

        protected static double boundingBoxTolerance = 1e-1;
        protected double centerOfMassTolerance = 1e-1;
        protected double extrusionTolerance = 1e-1;
        protected double distanceTolerance = 1e-1;
        protected double durationTolerance = 1e-1;

        public FeatureInfo(string fillType)
        {
            FillType = fillType;
        }

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

        public virtual void AssertEqualsExpected(IFeatureInfo other)
        {
            AssertEqualsExpected((FeatureInfo)other);
        }

        public virtual void AssertEqualsExpected(FeatureInfo expected)
        {
            if (!BoundingBox.Equals(expected.BoundingBox, boundingBoxTolerance))
                throw new FeatureBoundingBoxMismatch($"Bounding boxes aren't equal; expected {expected.BoundingBox}, got {BoundingBox}");

            if (!MathUtil.EpsilonEqual(Extrusion, expected.Extrusion, extrusionTolerance))
                throw new FeatureCumulativeExtrusionMismatch($"Cumulative extrusion amounts aren't equal; expected {expected.Extrusion}, got {Extrusion}");

            if (!MathUtil.EpsilonEqual(Duration, expected.Duration, durationTolerance))
                throw new FeatureCumulativeDurationMismatch($"Cumulative durations aren't equal; expected {expected.Duration}, got {Duration}");

            if (!MathUtil.EpsilonEqual(Distance, expected.Distance, distanceTolerance))
                throw new FeatureCumulativeDistanceMismatch($"Cumulative distances aren't equal; expected {expected.Distance}, got {Distance}");

            if (!CenterOfMass.EpsilonEqual(expected.CenterOfMass, centerOfMassTolerance))
                throw new FeatureCenterOfMassMismatch($"Centers of mass aren't equal; expected {expected.CenterOfMass}, got {CenterOfMass}");
        }

        public virtual void Add(FeatureInfo other)
        {
            BoundingBox.Contain(other.BoundingBox);
            Extrusion += other.Extrusion;
            Duration += other.Duration;
            Distance += other.Distance;
            UnweightedCenterOfMass += other.UnweightedCenterOfMass;
        }
    }
}