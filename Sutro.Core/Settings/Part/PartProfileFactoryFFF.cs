using Sutro.Core.Settings.Machine;
using System.Collections.Generic;

namespace Sutro.Core.Settings.Part
{
    public static class PartProfileFactoryFFF
    {
        public static IEnumerable<PartProfileFFF> EnumerateDefaults()
        {
            yield return new PartProfileFFF().ConfigureCommon().ConfigureStandard();
            yield return new PartProfileFFF().ConfigureCommon().ConfigureDraft();
            yield return new PartProfileFFF().ConfigureCommon().ConfigureStrong();
            yield return new PartProfileFFF().ConfigureCommon().ConfigureQuality();
        }

        private static PartProfileFFF ConfigureCommon(this PartProfileFFF part)
        {
            return part;
        }

        private static PartProfileFFF ConfigureDraft(this PartProfileFFF part)
        {
            part.Name = "Draft";
            part.LayerHeightMM = 0.3;
            return part;
        }

        private static PartProfileFFF ConfigureQuality(this PartProfileFFF part)
        {
            part.Name = "High Quality";
            part.LayerHeightMM = 0.1;
            return part;
        }

        private static PartProfileFFF ConfigureStandard(this PartProfileFFF part)
        {
            part.Name = "Standard";
            part.LayerHeightMM = 0.2;
            return part;
        }

        private static PartProfileFFF ConfigureStrong(this PartProfileFFF part)
        {
            part.Name = "Strong";
            part.LayerHeightMM = 0.25;
            part.Shells = 4;
            part.FloorLayers = 4;
            part.RoofLayers = 4;
            return part;
        }

        public static void ApplyMaxMachineSpeeds(PartProfileFFF part, MachineProfileFFF machine)
        {
            part.RapidExtrudeSpeed = machine.MaxExtrudeSpeedMMM;
            part.RapidTravelSpeed = machine.MaxTravelSpeedMMM;
            part.RetractSpeed = machine.MaxRetractSpeedMMM;
            part.ZTravelSpeed = machine.MaxZTravelSpeedMMM;
        }
    }
}