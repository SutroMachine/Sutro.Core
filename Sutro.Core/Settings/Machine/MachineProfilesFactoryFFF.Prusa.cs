using System.Collections.Generic;

namespace Sutro.Core.Settings.Machine
{
    public static partial class MachineProfilesFactoryFFF
    {
        public static class Prusa
        {
            private static void ConfigureCommon(MachineProfileFFF profile)
            {
                profile.Class = MachineClass.PlasticFFFPrinter;
                profile.Firmware = FirmwareOptions.Prusa;
                profile.ManufacturerName = "Prusa";
            }

            public static MachineProfileFFF Create_i3Mk3()
            {
                var profile = new MachineProfileFFF();
                ConfigureCommon(profile);
                Configure_i3Mk3(profile);
                return profile;
            }

            public static IEnumerable<MachineProfileFFF> EnumerateDefaults()
            {
                yield return Create_i3Mk3();
            }

            private static void Configure_i3Mk3(MachineProfileFFF profile)
            {
                profile.Name = "Prusa i3 Mk3";
                profile.ModelIdentifier = "i3 MK3";
                profile.BedSizeXMM = 250;
                profile.BedSizeYMM = 210;
                profile.MaxHeightMM = 200;
                profile.NozzleDiamMM = 0.4;

                profile.MaxExtruderTempC = 300;
                profile.HasHeatedBed = true;
                profile.MaxBedTempC = 120;

                profile.HasAutoBedLeveling = true;
                profile.EnableAutoBedLeveling = true;

                profile.MaxExtrudeSpeedMMM = 80 * 60;
                profile.MaxTravelSpeedMMM = 120 * 60;
                profile.MaxZTravelSpeedMMM = 250 * 60;
                profile.MaxRetractSpeedMMM = 35 * 60;
                profile.MinLayerHeightMM = 0.05;
                profile.MaxLayerHeightMM = 0.35;
            }
        }
    }
}