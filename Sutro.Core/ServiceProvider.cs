using Sutro.Core.Parallel;
using Sutro.Core.Settings;
using Sutro.Core.Support;
using System;

namespace Sutro.Core
{
    /// <summary>
    /// This is a utility class that instantiates services based on a print profile,
    /// allowing services to be decoupled from the print profile class.
    /// </summary>
    public class ServiceProvider
    {
        private readonly IPrintProfileFFF profile;

        public ServiceProvider(IPrintProfileFFF profile)
        {
            this.profile = profile;
        }

        public ISupportPointGenerator GetSupportPointGenerator()
        {
            return new CircularSupportPointGenerator(
                profile.Part.SupportPointDiam,
                profile.Part.SupportPointSides);
        }

        public LayerSupportCalculator GetLayerSupportCalculator()
        {
            double supportOffset = profile.Part.SupportAreaOffsetX * profile.Machine.NozzleDiamMM;
            return new LayerSupportCalculator(supportOffset, profile.SupportOverhangDistance());
        }

        public SupportAreaGenerator GetSupportAreaGenerator()
        {
            double discardHoleSizeMM = 2 * profile.Machine.NozzleDiamMM;
            double discardHoleArea = discardHoleSizeMM * discardHoleSizeMM;


            return new SupportAreaGenerator(
                layerSupportCalculator: GetLayerSupportCalculator(),
                supportPointGenerator: GetSupportPointGenerator(),
                parallelActor: GetParallelActor(),
                printWidth: profile.Machine.NozzleDiamMM,
                mergeDownDilate: profile.Machine.NozzleDiamMM * profile.Part.SupportRegionJoinTolX,
                supportGap: profile.Part.SupportSolidSpace,
                discardHoleArea: discardHoleArea,
                discardHoleSize: discardHoleSizeMM,
                minDiameter: profile.Part.SupportMinDimension,
                supportMinDist: profile.Machine.NozzleDiamMM,
                supportMinZTips: profile.Part.SupportMinZTips);

        }

        public virtual IParallelActor GetParallelActor()
        {
            return new ParallelActorAll();
        }
    }
}