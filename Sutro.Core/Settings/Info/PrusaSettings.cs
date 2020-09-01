using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings.Machine;
using System.Collections.Generic;

namespace Sutro.Core.Settings.Info
{
    public class PrusaSettings : GenericRepRapSettings
    {
        public override IPrintProfile Clone()
        {
            return CloneAs<PrusaSettings>();
        }

        public PrusaSettings() : base()
        {
            Machine.Firmware = FirmwareOptions.Prusa;
            ConfigureUnknown();
        }

        public static PrusaSettings Create_Generic()
        {
            return new PrusaSettings();
        }

        public static PrusaSettings Create_i3MK3()
        {
            var settings = new PrusaSettings();
            settings.Configure_i3_MK3();
            return settings;
        }

        public static IEnumerable<PrintProfileFFF> EnumerateDefaults()
        {
            yield return Create_i3MK3();
            yield return Create_Generic();
        }

        private void Configure_i3_MK3()
        {
            Machine.ManufacturerName = "Prusa";
            Machine.ModelIdentifier = "i3 MK3";
            Machine.Class = MachineClass.PlasticFFFPrinter;
            Machine.BedSizeXMM = 250;
            Machine.BedSizeYMM = 210;
            Machine.MaxHeightMM = 200;
            Machine.NozzleDiamMM = 0.4;

            Machine.MaxExtruderTempC = 300;
            Machine.HasHeatedBed = true;
            Machine.MaxBedTempC = 120;

            Machine.HasAutoBedLeveling = true;
            Machine.EnableAutoBedLeveling = true;

            Machine.MaxExtrudeSpeedMMM = 80 * 60;
            Machine.MaxTravelSpeedMMM = 120 * 60;
            Machine.MaxZTravelSpeedMMM = 250 * 60;
            Machine.MaxRetractSpeedMMM = 35 * 60;
            Machine.MinLayerHeightMM = 0.05;
            Machine.MaxLayerHeightMM = 0.35;

            Material.FilamentDiamMM = 1.75;

            Part.LayerHeightMM = 0.2;

            Material.ExtruderTempC = 200;
            Material.HeatedBedTempC = 60;

            Part.SolidFillNozzleDiamStepX = 1.0;
            Part.RetractDistanceMM = 0.7;

            Part.RetractSpeed = Machine.MaxRetractSpeedMMM;
            Part.ZTravelSpeed = Machine.MaxZTravelSpeedMMM;
            Part.RapidTravelSpeed = Machine.MaxTravelSpeedMMM;
            Part.CarefulExtrudeSpeed = 20 * 60;
            Part.RapidExtrudeSpeed = Machine.MaxExtrudeSpeedMMM;
            Part.OuterPerimeterSpeedX = 0.5;
        }

        private void ConfigureUnknown()
        {
            Machine.ManufacturerName = "Prusa";
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