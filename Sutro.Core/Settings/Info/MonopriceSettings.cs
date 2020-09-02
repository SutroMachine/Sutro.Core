using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings.Machine;
using System.Collections.Generic;

namespace Sutro.Core.Settings.Info
{
    public class MonopriceSettings : GenericRepRapSettings
    {
        public override IPrintProfile Clone()
        {
            return SettingsPrototype.CloneAs<MonopriceSettings, MonopriceSettings>(this);
        }

        public MonopriceSettings() : base()
        {
            Machine.Firmware = FirmwareOptions.Monoprice;
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
            Machine.ManufacturerName = "Monoprice";
            Machine.ModelIdentifier = "MP Select Mini V2";
            Machine.Class = MachineClass.PlasticFFFPrinter;
            Machine.BedSizeXMM = 120;
            Machine.BedSizeYMM = 120;
            Machine.MaxHeightMM = 120;
            Machine.NozzleDiamMM = 0.4;
            Material.FilamentDiamMM = 1.75;

            Machine.MaxExtruderTempC = 250;
            Machine.HasHeatedBed = true;
            Machine.MaxBedTempC = 60;

            Machine.MaxExtrudeSpeedMMM = 55 * 60;
            Machine.MaxTravelSpeedMMM = 150 * 60;
            Machine.MaxZTravelSpeedMMM = 100 * 60;
            Machine.MaxRetractSpeedMMM = 100 * 60;
            Machine.MinLayerHeightMM = 0.1;
            Machine.MaxLayerHeightMM = 0.3;

            Part.LayerHeightMM = 0.2;

            Material.ExtruderTempC = 200;
            Material.HeatedBedTempC = 0;

            Part.SolidFillNozzleDiamStepX = 1.0;
            Part.RetractDistanceMM = 4.5;

            Part.RetractSpeed = Machine.MaxRetractSpeedMMM;
            Part.ZTravelSpeed = Machine.MaxZTravelSpeedMMM;
            Part.RapidTravelSpeed = Machine.MaxTravelSpeedMMM;
            Part.CarefulExtrudeSpeed = 20 * 60;
            Part.RapidExtrudeSpeed = Machine.MaxExtrudeSpeedMMM;
            Part.OuterPerimeterSpeedX = 0.5;
        }

        private void ConfigureGeneric()
        {
            Machine.ManufacturerName = "Monoprice";
            Machine.ModelIdentifier = "Generic";
            Machine.Class = MachineClass.PlasticFFFPrinter;
            Machine.BedSizeXMM = 100;
            Machine.BedSizeYMM = 100;
            Machine.MaxHeightMM = 100;
            Machine.NozzleDiamMM = 0.4;
            Material.FilamentDiamMM = 1.75;

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