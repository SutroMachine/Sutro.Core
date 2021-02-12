using Sutro.Core.Models.Profiles;
using System.Collections.Generic;

namespace Sutro.Core.Settings.Machine
{
    public static partial class MachineProfilesFactoryFFF
    {
        public static class RepRap
        {
            private static void ConfigureCommon(MachineProfileFFF profile)
            {
                profile.Class = MachineClass.PlasticFFFPrinter;
                profile.Firmware = FirmwareOptions.RepRap;
                profile.ManufacturerName = "RepRap";
            }

            public static MachineProfileFFF CreateGeneric()
            {
                var profile = new MachineProfileFFF();
                ConfigureCommon(profile);
                ConfigureGeneric(profile);
                return profile;
            }

            public static IEnumerable<MachineProfileFFF> EnumerateDefaults()
            {
                yield return CreateGeneric();
            }

            private static void ConfigureGeneric(MachineProfileFFF profile)
            {
                profile.ModelIdentifier = "Generic RepRap";

                profile.BedSizeXMM = 80;
                profile.BedSizeYMM = 80;
                profile.OriginX = MachineBedOriginLocationX.Center;
                profile.OriginY = MachineBedOriginLocationY.Center;

                profile.MaxHeightMM = 55;
                profile.NozzleDiamMM = 0.4;

                profile.MaxExtruderTempC = 230;
                profile.HasHeatedBed = false;
                profile.MaxBedTempC = 60;

                profile.MaxExtrudeSpeedMMM = 50 * 60;
                profile.MaxTravelSpeedMMM = 150 * 60;
                profile.MaxZTravelSpeedMMM = 100 * 60;
                profile.MaxRetractSpeedMMM = 40 * 60;
                profile.MinLayerHeightMM = 0.1;
                profile.MaxLayerHeightMM = 0.3;
            }
        }
    }
}