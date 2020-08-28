using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings.Machine;
using System.Collections.Generic;

namespace Sutro.Core.Settings.Info
{
    public class PrintrbotSettings : GenericRepRapSettings
    {
        public override IPrintProfile Clone()
        {
            return CloneAs<PrintrbotSettings>();
        }

        public PrintrbotSettings() : base()
        {
            MachineProfile.Firmware = FirmwareOptions.Printrbot;
            ConfigureUnknown();
        }

        public static PrintrbotSettings CreateGeneric()
        {
            return new PrintrbotSettings();
        }

        public static PrintrbotSettings CreatePlus()
        {
            var settings = new PrintrbotSettings();
            settings.ConfigurePlus();
            return settings;
        }

        public static IEnumerable<PrintProfileFFF> EnumerateDefaults()
        {
            yield return PrintrbotSettings.CreatePlus();
            yield return PrintrbotSettings.CreateGeneric();
        }


        private void ConfigureCommon()
        {
            MachineProfile.Class = MachineClass.PlasticFFFPrinter;
            MachineProfile.Firmware = FirmwareOptions.Printrbot;
            MachineProfile.ManufacturerName = "Printrbot";
            MachineProfile.NozzleDiamMM = 0.4;
            MaterialProfile.FilamentDiamMM = 1.75;
        }

        private void ConfigurePlus()
        {
            ConfigureCommon();
            MachineProfile.ModelIdentifier = "Plus";
            MachineProfile.BedSizeXMM = 250;
            MachineProfile.BedSizeYMM = 250;
            MachineProfile.MaxHeightMM = 250;

            MachineProfile.MaxExtruderTempC = 250;
            MachineProfile.HasHeatedBed = true;
            MachineProfile.MaxBedTempC = 80;

            MachineProfile.MaxExtrudeSpeedMMM = 80 * 60;
            MachineProfile.MaxTravelSpeedMMM = 120 * 60;
            MachineProfile.MaxZTravelSpeedMMM = 100 * 60;
            MachineProfile.MaxRetractSpeedMMM = 45 * 60;
            MachineProfile.MinLayerHeightMM = 0.05;
            MachineProfile.MaxLayerHeightMM = 0.3;

            PartProfile.LayerHeightMM = 0.2;

            MaterialProfile.ExtruderTempC = 200;
            MaterialProfile.HeatedBedTempC = 0;

            PartProfile.SolidFillNozzleDiamStepX = 1.0;
            PartProfile.RetractDistanceMM = 0.7;

            PartProfile.RetractSpeed = MachineProfile.MaxRetractSpeedMMM;
            PartProfile.ZTravelSpeed = MachineProfile.MaxZTravelSpeedMMM;
            PartProfile.RapidTravelSpeed = MachineProfile.MaxTravelSpeedMMM;
            PartProfile.CarefulExtrudeSpeed = 20 * 60;
            PartProfile.RapidExtrudeSpeed = MachineProfile.MaxExtrudeSpeedMMM;
            PartProfile.OuterPerimeterSpeedX = 0.5;

            // Specific to printrbot
            MachineProfile.HasAutoBedLeveling = true;
            MachineProfile.EnableAutoBedLeveling = true;
        }

        private void ConfigureUnknown()
        {
            ConfigureCommon();

            MachineProfile.ModelIdentifier = "Generic";

            MachineProfile.BedSizeXMM = 100;
            MachineProfile.BedSizeYMM = 100;
            MachineProfile.MaxHeightMM = 100;

            MachineProfile.MaxExtruderTempC = 230;
            MachineProfile.HasHeatedBed = false;
            MachineProfile.MaxBedTempC = 0;

            MachineProfile.HasAutoBedLeveling = false;
            MachineProfile.EnableAutoBedLeveling = false;

            MachineProfile.MaxExtrudeSpeedMMM = 60 * 60;
            MachineProfile.MaxTravelSpeedMMM = 80 * 60;
            MachineProfile.MaxZTravelSpeedMMM = 23 * 60;
            MachineProfile.MaxRetractSpeedMMM = 20 * 60;
            MachineProfile.MinLayerHeightMM = 0.1;
            MachineProfile.MaxLayerHeightMM = 0.3;

            PartProfile.LayerHeightMM = 0.2;

            MaterialProfile.ExtruderTempC = 200;
            MaterialProfile.HeatedBedTempC = 0;

            PartProfile.SolidFillNozzleDiamStepX = 1.0;
            PartProfile.RetractDistanceMM = 1.0;

            PartProfile.RetractSpeed = MachineProfile.MaxRetractSpeedMMM;
            PartProfile.ZTravelSpeed = MachineProfile.MaxZTravelSpeedMMM;
            PartProfile.RapidTravelSpeed = MachineProfile.MaxTravelSpeedMMM;
            PartProfile.CarefulExtrudeSpeed = 20 * 60;
            PartProfile.RapidExtrudeSpeed = MachineProfile.MaxExtrudeSpeedMMM;
            PartProfile.OuterPerimeterSpeedX = 0.5;
        }
    }
}