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
            MachineProfile.Firmware = FirmwareOptions.Prusa;
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
            MachineProfile.ManufacturerName = "Prusa";
            MachineProfile.ModelIdentifier = "i3 MK3";
            MachineProfile.Class = MachineClass.PlasticFFFPrinter;
            MachineProfile.BedSizeXMM = 250;
            MachineProfile.BedSizeYMM = 210;
            MachineProfile.MaxHeightMM = 200;
            MachineProfile.NozzleDiamMM = 0.4;

            MachineProfile.MaxExtruderTempC = 300;
            MachineProfile.HasHeatedBed = true;
            MachineProfile.MaxBedTempC = 120;

            MachineProfile.HasAutoBedLeveling = true;
            MachineProfile.EnableAutoBedLeveling = true;

            MachineProfile.MaxExtrudeSpeedMMM = 80 * 60;
            MachineProfile.MaxTravelSpeedMMM = 120 * 60;
            MachineProfile.MaxZTravelSpeedMMM = 250 * 60;
            MachineProfile.MaxRetractSpeedMMM = 35 * 60;
            MachineProfile.MinLayerHeightMM = 0.05;
            MachineProfile.MaxLayerHeightMM = 0.35;

            MaterialProfile.FilamentDiamMM = 1.75;

            PartProfile.LayerHeightMM = 0.2;

            MaterialProfile.ExtruderTempC = 200;
            MaterialProfile.HeatedBedTempC = 60;

            PartProfile.SolidFillNozzleDiamStepX = 1.0;
            PartProfile.RetractDistanceMM = 0.7;

            PartProfile.RetractSpeed = MachineProfile.MaxRetractSpeedMMM;
            PartProfile.ZTravelSpeed = MachineProfile.MaxZTravelSpeedMMM;
            PartProfile.RapidTravelSpeed = MachineProfile.MaxTravelSpeedMMM;
            PartProfile.CarefulExtrudeSpeed = 20 * 60;
            PartProfile.RapidExtrudeSpeed = MachineProfile.MaxExtrudeSpeedMMM;
            PartProfile.OuterPerimeterSpeedX = 0.5;
        }

        private void ConfigureUnknown()
        {
            MachineProfile.ManufacturerName = "Prusa";
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