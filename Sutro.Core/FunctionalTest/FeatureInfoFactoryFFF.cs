using g3;
using gs;
using Sutro.Core.Models.GCode;
using System.Collections.Generic;

namespace Sutro.Core.FunctionalTest
{
    public class FeatureInfoFactoryFFF : IFeatureInfoFactory<FeatureInfo>
    {
        protected PrintVertex VertexPrevious;
        protected PrintVertex VertexCurrent;

        protected FeatureInfo currentFeatureInfo;

        protected readonly List<string> endFeatureComments = new List<string>()
        {
            "retract", "travel"
        };

        public FeatureInfo SwitchFeature(string featureType)
        {
            var result = currentFeatureInfo;
            currentFeatureInfo = new FeatureInfo(featureType);
            if (result?.Extrusion > 0)
                return result;
            else
                return null;
        }

        public virtual void ObserveGcodeLine(GCodeLine line)
        {
            if (line.Type != LineType.GCode)
                return;

            double x = VertexPrevious.Position.x;
            double y = VertexPrevious.Position.y;

            GCodeUtil.TryFindParamNum(line.Parameters, "X", ref x);
            GCodeUtil.TryFindParamNum(line.Parameters, "Y", ref y);

            VertexCurrent.Position = new Vector3d(x, y, 0);

            double f = GCodeUtil.UnspecifiedValue;
            if (GCodeUtil.TryFindParamNum(line.Parameters, "F", ref f))
                VertexCurrent.FeedRate = f;

            double extrusionAmount = GCodeUtil.UnspecifiedValue;
            bool featureActive = GCodeUtil.TryFindParamNum(line.Parameters, "E", ref extrusionAmount) &&
                                 extrusionAmount > VertexPrevious.Extrusion.x &&
                                 currentFeatureInfo != null;

            foreach (var s in endFeatureComments)
                if (!string.IsNullOrWhiteSpace(line.Comment) && line.Comment.ToLower().Contains(s))
                    featureActive = false;

            if (featureActive)
            {
                Vector2d average = new Segment2d(VertexCurrent.Position.xy, VertexPrevious.Position.xy).Center;
                double distance = VertexCurrent.Position.Distance(VertexPrevious.Position);
                double extrusion = extrusionAmount - VertexPrevious.Extrusion.x;

                currentFeatureInfo.Extrusion += extrusion;
                currentFeatureInfo.Distance += distance;
                currentFeatureInfo.BoundingBox.Contain(VertexPrevious.Position.xy);
                currentFeatureInfo.BoundingBox.Contain(VertexCurrent.Position.xy);
                currentFeatureInfo.UnweightedCenterOfMass += average * extrusion;
                currentFeatureInfo.Duration += 60 * distance / VertexCurrent.FeedRate;

                VertexCurrent.Extrusion = new Vector3d(extrusionAmount, 0, 0);
            }

            VertexPrevious = new PrintVertex(VertexCurrent);
        }

        public virtual void Initialize()
        {
            VertexPrevious = new PrintVertex(Vector3d.Zero, 0, Vector2d.Zero);
            VertexCurrent = new PrintVertex(VertexPrevious);
            currentFeatureInfo = null;
        }
    }
}