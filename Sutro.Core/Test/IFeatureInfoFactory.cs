using Sutro.Core.Models.GCode;

namespace Sutro.Core.Test
{
    public interface IFeatureInfoFactory<out TFeatureInfo> where TFeatureInfo : IFeatureInfo
    {
        TFeatureInfo SwitchFeature(string featureType);

        void ObserveGcodeLine(GCodeLine line);

        void Initialize();
    }
}