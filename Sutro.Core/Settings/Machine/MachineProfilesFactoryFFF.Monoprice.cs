using System.Collections.Generic;

namespace Sutro.Core.Settings.Machine
{
    public static partial class MachineProfilesFactoryFFF
    {
        public static class Monoprice
        {
            public static MachineProfileFFF CreateSelectMiniV2()
            {
                var profile = new MachineProfileFFF();
                ConfigureCommon(profile);
                ConfigureSelectMiniV2(profile);
                return profile;
            }

            public static IEnumerable<MachineProfileFFF> EnumerateDefaults()
            {
                yield return CreateSelectMiniV2();
            }

            private static void ConfigureCommon(MachineProfileFFF profile)
            {
                profile.Class = MachineClass.PlasticFFFPrinter;
                profile.Firmware = FirmwareOptions.Monoprice;
                profile.ManufacturerName = "Monoprice";
            }
          
            private static void ConfigureSelectMiniV2(MachineProfileFFF profile)
            {
                profile.Name = "Monoprice Select Mini V2";
                profile.ModelIdentifier = "MP Select Mini V2";
                profile.BedSizeXMM = 120;
                profile.BedSizeYMM = 120;
                profile.MaxHeightMM = 120;
                profile.NozzleDiamMM = 0.4;

                profile.MaxExtruderTempC = 250;
                profile.HasHeatedBed = true;
                profile.MaxBedTempC = 60;

                profile.MaxExtrudeSpeedMMM = 55 * 60;
                profile.MaxTravelSpeedMMM = 150 * 60;
                profile.MaxZTravelSpeedMMM = 100 * 60;
                profile.MaxRetractSpeedMMM = 100 * 60;
                profile.MinLayerHeightMM = 0.1;
                profile.MaxLayerHeightMM = 0.3;
            }
        }
    }
}