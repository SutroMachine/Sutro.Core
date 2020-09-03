using System.Collections.Generic;

namespace Sutro.Core.Settings.Machine
{
    public static partial class MachineProfilesFactoryFFF
    {
        public static class Makerbot
        {
            public static MachineProfileFFF CreateReplicator2()
            {
                var profile = new MachineProfileFFF();
                ConfigureCommon(profile);
                ConfigureReplicator2(profile);
                return profile;
            }

            public static IEnumerable<MachineProfileFFF> EnumerateDefaults()
            {
                yield return CreateReplicator2();
            }

            private static void ConfigureCommon(MachineProfileFFF profile)
            {
                profile.Class = MachineClass.PlasticFFFPrinter;
                profile.Firmware = FirmwareOptions.Makerbot;
                profile.ManufacturerName = "Makerbot";
            }

            private static void ConfigureReplicator2(MachineProfileFFF profile)
            {
                profile.Name = "Makerbot Replicator 2";
                profile.ModelIdentifier = "Replicator 2";
                profile.BedSizeXMM = 285;
                profile.BedSizeYMM = 153;
                profile.MaxHeightMM = 155;
                profile.NozzleDiamMM = 0.4;

                profile.MaxExtruderTempC = 230;
                profile.HasHeatedBed = false;
                profile.MaxBedTempC = 0;

                profile.MaxExtrudeSpeedMMM = 90 * 60;
                profile.MaxTravelSpeedMMM = 150 * 60;
                profile.MaxZTravelSpeedMMM = 23 * 60;
                profile.MaxRetractSpeedMMM = 25 * 60;
                profile.MinLayerHeightMM = 0.1;
                profile.MaxLayerHeightMM = 0.3;
            }
        }
    }
}