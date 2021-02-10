using System.Collections.Generic;

namespace Sutro.Core.Settings.Machine
{
    public static partial class MachineProfilesFactoryFFF
    {
        public static class Flashforge
        {
            public static MachineProfileFFF CreateCreatorPro()
            {
                var profile = new MachineProfileFFF();
                ConfigureCommon(profile);
                ConfigureCreatorPro(profile);
                return profile;
            }

            public static IEnumerable<MachineProfileFFF> EnumerateDefaults()
            {
                yield return CreateCreatorPro();
            }

            private static void ConfigureCommon(MachineProfileFFF profile)
            {
                profile.Class = MachineClass.PlasticFFFPrinter;
                profile.Firmware = FirmwareOptions.Flashforge;
                profile.ManufacturerName = "Flashforge";
            }

            private static void ConfigureCreatorPro(MachineProfileFFF profile)
            {
                profile.Name = "Flashforge Creater Pro";
                profile.ModelIdentifier = "Creator Pro";
                profile.BedSizeXMM = 227;
                profile.BedSizeYMM = 148;
                profile.MaxHeightMM = 150;
                profile.NozzleDiamMM = 0.4;

                profile.MaxExtruderTempC = 230;
                profile.HasHeatedBed = true;
                profile.MaxBedTempC = 105;

                profile.MaxExtrudeSpeedMMM = 60 * 60;
                profile.MaxTravelSpeedMMM = 80 * 60;
                profile.MaxZTravelSpeedMMM = 23 * 60;
                profile.MaxRetractSpeedMMM = 20 * 60;
                profile.MinLayerHeightMM = 0.1;
                profile.MaxLayerHeightMM = 0.3;
            }
        }
    }
}