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
            MachineProfile.Firmware = FirmwareOptions.Flashforge;
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
            MachineProfile.ManufacturerName = "Flashforge";
            MachineProfile.ModelIdentifier = "Creator Pro";
            MachineProfile.Class = MachineClass.PlasticFFFPrinter;
            MachineProfile.BedSizeXMM = 227;
            MachineProfile.BedSizeYMM = 148;
            MachineProfile.MaxHeightMM = 150;
            MachineProfile.NozzleDiamMM = 0.4;
            MaterialProfile.FilamentDiamMM = 1.75;

            MachineProfile.MaxExtruderTempC = 230;
            MachineProfile.HasHeatedBed = true;
            MachineProfile.MaxBedTempC = 105;

            MachineProfile.MaxExtrudeSpeedMMM = 60 * 60;
            MachineProfile.MaxTravelSpeedMMM = 80 * 60;
            MachineProfile.MaxZTravelSpeedMMM = 23 * 60;
            MachineProfile.MaxRetractSpeedMMM = 20 * 60;
            MachineProfile.MinLayerHeightMM = 0.1;
            MachineProfile.MaxLayerHeightMM = 0.3;

            PartProfile.LayerHeightMM = 0.2;

            MaterialProfile.ExtruderTempC = 230;
            MaterialProfile.HeatedBedTempC = 25;

            PartProfile.SolidFillNozzleDiamStepX = 1.0;
            PartProfile.RetractDistanceMM = 1.3;

            PartProfile.RetractSpeed = MachineProfile.MaxRetractSpeedMMM;
            PartProfile.ZTravelSpeed = MachineProfile.MaxZTravelSpeedMMM;
            PartProfile.RapidTravelSpeed = MachineProfile.MaxTravelSpeedMMM;
            PartProfile.CarefulExtrudeSpeed = 30 * 60;
            PartProfile.RapidExtrudeSpeed = MachineProfile.MaxExtrudeSpeedMMM;
            PartProfile.OuterPerimeterSpeedX = 0.5;
        }

        private void ConfigureUnknown()
        {
            MachineProfile.ManufacturerName = "Flashforge";
            MachineProfile.ModelIdentifier = "Generic";
            MachineProfile.Class = MachineClass.PlasticFFFPrinter;

            MachineProfile.BedSizeXMM = 100;
            MachineProfile.BedSizeYMM = 100;
            MachineProfile.MaxHeightMM = 130;
            MachineProfile.NozzleDiamMM = 0.4;
            MaterialProfile.FilamentDiamMM = 1.75;

            MachineProfile.MaxExtruderTempC = 230;
            MachineProfile.HasHeatedBed = false;
            MachineProfile.MaxBedTempC = 0;

            MachineProfile.MaxExtrudeSpeedMMM = 60 * 60;
            MachineProfile.MaxTravelSpeedMMM = 80 * 60;
            MachineProfile.MaxZTravelSpeedMMM = 23 * 60;
            MachineProfile.MaxRetractSpeedMMM = 20 * 60;
            MachineProfile.MinLayerHeightMM = 0.1;
            MachineProfile.MaxLayerHeightMM = 0.3;

            PartProfile.LayerHeightMM = 0.2;

            MaterialProfile.ExtruderTempC = 230;
            MaterialProfile.HeatedBedTempC = 0;

            PartProfile.SolidFillNozzleDiamStepX = 1.0;
            PartProfile.RetractDistanceMM = 1.3;

            PartProfile.RetractSpeed = MachineProfile.MaxRetractSpeedMMM;
            PartProfile.ZTravelSpeed = MachineProfile.MaxZTravelSpeedMMM;
            PartProfile.RapidTravelSpeed = MachineProfile.MaxTravelSpeedMMM;
            PartProfile.CarefulExtrudeSpeed = 30 * 60;
            PartProfile.RapidExtrudeSpeed = MachineProfile.MaxExtrudeSpeedMMM;
            PartProfile.OuterPerimeterSpeedX = 0.5;
        }
    }
}