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
            Machine.Firmware = FirmwareOptions.Printrbot;
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
            Machine.Class = MachineClass.PlasticFFFPrinter;
            Machine.Firmware = FirmwareOptions.Printrbot;
            Machine.ManufacturerName = "Printrbot";
            Machine.NozzleDiamMM = 0.4;
            Material.FilamentDiamMM = 1.75;
        }

        private void ConfigurePlus()
        {
            ConfigureCommon();
            Machine.ModelIdentifier = "Plus";
            Machine.BedSizeXMM = 250;
            Machine.BedSizeYMM = 250;
            Machine.MaxHeightMM = 250;

            Machine.MaxExtruderTempC = 250;
            Machine.HasHeatedBed = true;
            Machine.MaxBedTempC = 80;

            Machine.MaxExtrudeSpeedMMM = 80 * 60;
            Machine.MaxTravelSpeedMMM = 120 * 60;
            Machine.MaxZTravelSpeedMMM = 100 * 60;
            Machine.MaxRetractSpeedMMM = 45 * 60;
            Machine.MinLayerHeightMM = 0.05;
            Machine.MaxLayerHeightMM = 0.3;

            Part.LayerHeightMM = 0.2;

            Material.ExtruderTempC = 200;
            Material.HeatedBedTempC = 0;

            Part.SolidFillNozzleDiamStepX = 1.0;
            Part.RetractDistanceMM = 0.7;

            Part.RetractSpeed = Machine.MaxRetractSpeedMMM;
            Part.ZTravelSpeed = Machine.MaxZTravelSpeedMMM;
            Part.RapidTravelSpeed = Machine.MaxTravelSpeedMMM;
            Part.CarefulExtrudeSpeed = 20 * 60;
            Part.RapidExtrudeSpeed = Machine.MaxExtrudeSpeedMMM;
            Part.OuterPerimeterSpeedX = 0.5;

            // Specific to printrbot
            Machine.HasAutoBedLeveling = true;
            Machine.EnableAutoBedLeveling = true;
        }

        private void ConfigureUnknown()
        {
            ConfigureCommon();

            Machine.ModelIdentifier = "Generic";

            Machine.BedSizeXMM = 100;
            Machine.BedSizeYMM = 100;
            Machine.MaxHeightMM = 100;

            Machine.MaxExtruderTempC = 230;
            Machine.HasHeatedBed = false;
            Machine.MaxBedTempC = 0;

            Machine.HasAutoBedLeveling = false;
            Machine.EnableAutoBedLeveling = false;

            Machine.MaxExtrudeSpeedMMM = 60 * 60;
            Machine.MaxTravelSpeedMMM = 80 * 60;
            Machine.MaxZTravelSpeedMMM = 23 * 60;
            Machine.MaxRetractSpeedMMM = 20 * 60;
            Machine.MinLayerHeightMM = 0.1;
            Machine.MaxLayerHeightMM = 0.3;

            Part.LayerHeightMM = 0.2;

            Material.ExtruderTempC = 200;
            Material.HeatedBedTempC = 0;

            Part.SolidFillNozzleDiamStepX = 1.0;
            Part.RetractDistanceMM = 1.0;

            Part.RetractSpeed = Machine.MaxRetractSpeedMMM;
            Part.ZTravelSpeed = Machine.MaxZTravelSpeedMMM;
            Part.RapidTravelSpeed = Machine.MaxTravelSpeedMMM;
            Part.CarefulExtrudeSpeed = 20 * 60;
            Part.RapidExtrudeSpeed = Machine.MaxExtrudeSpeedMMM;
            Part.OuterPerimeterSpeedX = 0.5;
        }
    }
}