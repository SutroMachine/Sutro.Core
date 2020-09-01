using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings.Machine;
using System.Collections.Generic;

namespace Sutro.Core.Settings.Info
{
    public class FlashforgeSettings : PrintProfileFFF
    {
        public override IPrintProfile Clone()
        {
            return CloneAs<FlashforgeSettings>();
        }

        public FlashforgeSettings() : base()
        {
            Machine.Firmware = FirmwareOptions.Flashforge;
            ConfigureUnknown();
        }

        public static FlashforgeSettings CreateGeneric()
        {
            return new FlashforgeSettings();
        }

        public static FlashforgeSettings CreateCreatorPro()
        {
            var settings = new FlashforgeSettings();
            settings.ConfigureCreatorPro();
            return settings;
        }

        public static IEnumerable<PrintProfileFFF> EnumerateDefaults()
        {
            yield return CreateGeneric();
            yield return CreateCreatorPro();
        }

        private void ConfigureCreatorPro()
        {
            Machine.ManufacturerName = "Flashforge";
            Machine.ModelIdentifier = "Creator Pro";
            Machine.Class = MachineClass.PlasticFFFPrinter;
            Machine.BedSizeXMM = 227;
            Machine.BedSizeYMM = 148;
            Machine.MaxHeightMM = 150;
            Machine.NozzleDiamMM = 0.4;
            Material.FilamentDiamMM = 1.75;

            Machine.MaxExtruderTempC = 230;
            Machine.HasHeatedBed = true;
            Machine.MaxBedTempC = 105;

            Machine.MaxExtrudeSpeedMMM = 60 * 60;
            Machine.MaxTravelSpeedMMM = 80 * 60;
            Machine.MaxZTravelSpeedMMM = 23 * 60;
            Machine.MaxRetractSpeedMMM = 20 * 60;
            Machine.MinLayerHeightMM = 0.1;
            Machine.MaxLayerHeightMM = 0.3;

            Part.LayerHeightMM = 0.2;

            Material.ExtruderTempC = 230;
            Material.HeatedBedTempC = 25;

            Part.SolidFillNozzleDiamStepX = 1.0;
            Part.RetractDistanceMM = 1.3;

            Part.RetractSpeed = Machine.MaxRetractSpeedMMM;
            Part.ZTravelSpeed = Machine.MaxZTravelSpeedMMM;
            Part.RapidTravelSpeed = Machine.MaxTravelSpeedMMM;
            Part.CarefulExtrudeSpeed = 30 * 60;
            Part.RapidExtrudeSpeed = Machine.MaxExtrudeSpeedMMM;
            Part.OuterPerimeterSpeedX = 0.5;
        }

        private void ConfigureUnknown()
        {
            Machine.ManufacturerName = "Flashforge";
            Machine.ModelIdentifier = "Generic";
            Machine.Class = MachineClass.PlasticFFFPrinter;

            Machine.BedSizeXMM = 100;
            Machine.BedSizeYMM = 100;
            Machine.MaxHeightMM = 130;
            Machine.NozzleDiamMM = 0.4;
            Material.FilamentDiamMM = 1.75;

            Machine.MaxExtruderTempC = 230;
            Machine.HasHeatedBed = false;
            Machine.MaxBedTempC = 0;

            Machine.MaxExtrudeSpeedMMM = 60 * 60;
            Machine.MaxTravelSpeedMMM = 80 * 60;
            Machine.MaxZTravelSpeedMMM = 23 * 60;
            Machine.MaxRetractSpeedMMM = 20 * 60;
            Machine.MinLayerHeightMM = 0.1;
            Machine.MaxLayerHeightMM = 0.3;

            Part.LayerHeightMM = 0.2;

            Material.ExtruderTempC = 230;
            Material.HeatedBedTempC = 0;

            Part.SolidFillNozzleDiamStepX = 1.0;
            Part.RetractDistanceMM = 1.3;

            Part.RetractSpeed = Machine.MaxRetractSpeedMMM;
            Part.ZTravelSpeed = Machine.MaxZTravelSpeedMMM;
            Part.RapidTravelSpeed = Machine.MaxTravelSpeedMMM;
            Part.CarefulExtrudeSpeed = 30 * 60;
            Part.RapidExtrudeSpeed = Machine.MaxExtrudeSpeedMMM;
            Part.OuterPerimeterSpeedX = 0.5;
        }
    }
}