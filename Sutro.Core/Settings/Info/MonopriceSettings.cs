using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings.Machine;
using System.Collections.Generic;

namespace Sutro.Core.Settings.Info
{
    public class MonopriceSettings : GenericRepRapSettings
    {
        public override IPrintProfile Clone()
        {
            return CloneAs<MonopriceSettings>();
        }

        public MonopriceSettings() : base()
        {
            MachineProfile.Firmware = FirmwareOptions.Monoprice;
            ConfigureGeneric();
        }

        public static MonopriceSettings CreateGeneric()
        {
            var settings = new MonopriceSettings();
            settings.ConfigureGeneric();
            return settings;
        }

        public static MonopriceSettings CreateSelectMiniV2()
        {
            var settings = new MonopriceSettings();
            settings.ConfigureSelectMiniV2();
            return settings;
        }

        public static IEnumerable<PrintProfileFFF> EnumerateDefaults()
        {
            yield return CreateSelectMiniV2();
            yield return CreateGeneric();
        }

        private void ConfigureSelectMiniV2()
        {
            MachineProfile.ManufacturerName = "Monoprice";
            MachineProfile.ModelIdentifier = "MP Select Mini V2";
            MachineProfile.Class = MachineClass.PlasticFFFPrinter;
            MachineProfile.BedSizeXMM = 120;
            MachineProfile.BedSizeYMM = 120;
            MachineProfile.MaxHeightMM = 120;
            MachineProfile.NozzleDiamMM = 0.4;
            MaterialProfile.FilamentDiamMM = 1.75;

            MachineProfile.MaxExtruderTempC = 250;
            MachineProfile.HasHeatedBed = true;
            MachineProfile.MaxBedTempC = 60;

            MachineProfile.MaxExtrudeSpeedMMM = 55 * 60;
            MachineProfile.MaxTravelSpeedMMM = 150 * 60;
            MachineProfile.MaxZTravelSpeedMMM = 100 * 60;
            MachineProfile.MaxRetractSpeedMMM = 100 * 60;
            MachineProfile.MinLayerHeightMM = 0.1;
            MachineProfile.MaxLayerHeightMM = 0.3;

            PartProfile.LayerHeightMM = 0.2;

            MaterialProfile.ExtruderTempC = 200;
            MaterialProfile.HeatedBedTempC = 0;

            PartProfile.SolidFillNozzleDiamStepX = 1.0;
            PartProfile.RetractDistanceMM = 4.5;

            PartProfile.RetractSpeed = MachineProfile.MaxRetractSpeedMMM;
            PartProfile.ZTravelSpeed = MachineProfile.MaxZTravelSpeedMMM;
            PartProfile.RapidTravelSpeed = MachineProfile.MaxTravelSpeedMMM;
            PartProfile.CarefulExtrudeSpeed = 20 * 60;
            PartProfile.RapidExtrudeSpeed = MachineProfile.MaxExtrudeSpeedMMM;
            PartProfile.OuterPerimeterSpeedX = 0.5;
        }

        private void ConfigureGeneric()
        {
            MachineProfile.ManufacturerName = "Monoprice";
            MachineProfile.ModelIdentifier = "Generic";
            MachineProfile.Class = MachineClass.PlasticFFFPrinter;
            MachineProfile.BedSizeXMM = 100;
            MachineProfile.BedSizeYMM = 100;
            MachineProfile.MaxHeightMM = 100;
            MachineProfile.NozzleDiamMM = 0.4;
            MaterialProfile.FilamentDiamMM = 1.75;

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