using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings.Machine;
using System;
using System.Collections.Generic;

namespace Sutro.Core.Settings.Machine
{
    public static partial class MachineProfilesFactoryFFF
    {
        public static class Printrbot
        {
            private static void ConfigureCommon(MachineProfileFFF profile)
            {
                profile.Class = MachineClass.PlasticFFFPrinter;
                profile.Firmware = FirmwareOptions.Printrbot;
                profile.ManufacturerName = "Printrbot";
            }

            public static MachineProfileFFF CreateGeneric()
            {
                var profile = new MachineProfileFFF();
                ConfigureCommon(profile);
                ConfigureGeneric(profile);
                return profile;
            }

            public static MachineProfileFFF CreatePlus()
            {
                var profile = new MachineProfileFFF();
                ConfigureCommon(profile);
                ConfigurePlus(profile);
                return profile;
            }

            public static IEnumerable<MachineProfileFFF> EnumerateDefaults()
            {
                yield return CreateGeneric();
                yield return CreatePlus();
            }

            private static void ConfigurePlus(MachineProfileFFF profile)
            {
                profile.ModelIdentifier = "Plus";

                profile.BedSizeXMM = 250;
                profile.BedSizeYMM = 250;
                profile.MaxHeightMM = 250;

                profile.MaxExtruderTempC = 250;
                profile.HasHeatedBed = true;
                profile.MaxBedTempC = 80;

                profile.MaxExtrudeSpeedMMM = 80 * 60;
                profile.MaxTravelSpeedMMM = 120 * 60;
                profile.MaxZTravelSpeedMMM = 100 * 60;
                profile.MaxRetractSpeedMMM = 45 * 60;
                profile.MinLayerHeightMM = 0.05;
                profile.MaxLayerHeightMM = 0.3;

                profile.HasAutoBedLeveling = true;
                profile.EnableAutoBedLeveling = true;

            }

            private static void ConfigureGeneric(MachineProfileFFF profile)
            {
                profile.ModelIdentifier = "Generic";

                profile.BedSizeXMM = 100;
                profile.BedSizeYMM = 100;
                profile.MaxHeightMM = 100;

                profile.MaxExtruderTempC = 230;
                profile.HasHeatedBed = false;
                profile.MaxBedTempC = 0;

                profile.HasAutoBedLeveling = false;
                profile.EnableAutoBedLeveling = false;

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