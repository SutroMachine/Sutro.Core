using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings.Machine;
using System.Collections.Generic;

namespace Sutro.Core.Settings.Info
{
    public class MakerbotSettings : PrintProfileFFF
    {
        public override IPrintProfile Clone()
        {
            return CloneAs<MakerbotSettings>();
        }

        public MakerbotSettings() : base()
        {
            ConfigureCommon();
            ConfigureUnknown();
        }

        private void ConfigureCommon()
        {
            MachineProfile.Firmware = FirmwareOptions.Makerbot;
        }

        public static MakerbotSettings CreateGeneric()
        {
            return new MakerbotSettings();
        }

        public static MakerbotSettings CreateReplicator2()
        {
            var settings = new MakerbotSettings();
            settings.ConfigureReplicator2();
            return settings;
        }

        public static IEnumerable<PrintProfileFFF> EnumerateDefaults()
        {
            yield return CreateReplicator2();
            yield return CreateGeneric();
        }

        private void ConfigureReplicator2()
        {
            MachineProfile.ManufacturerName = "Makerbot";
            MachineProfile.ModelIdentifier = "Replicator 2";
            MachineProfile.Class = MachineClass.PlasticFFFPrinter;
            MachineProfile.BedSizeXMM = 285;
            MachineProfile.BedSizeYMM = 153;
            MachineProfile.MaxHeightMM = 155;
            MachineProfile.NozzleDiamMM = 0.4;
            MaterialProfile.FilamentDiamMM = 1.75;

            MachineProfile.MaxExtruderTempC = 230;
            MachineProfile.HasHeatedBed = false;
            MachineProfile.MaxBedTempC = 0;

            MachineProfile.MaxExtrudeSpeedMMM = 90 * 60;
            MachineProfile.MaxTravelSpeedMMM = 150 * 60;
            MachineProfile.MaxZTravelSpeedMMM = 23 * 60;
            MachineProfile.MaxRetractSpeedMMM = 25 * 60;
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

        private void ConfigureUnknown()
        {
            MachineProfile.ManufacturerName = "Makerbot";
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
            MachineProfile.MaxExtrudeSpeedMMM = 90 * 60;
            MachineProfile.MaxTravelSpeedMMM = 150 * 60;
            MachineProfile.MaxZTravelSpeedMMM = 23 * 60;
            MachineProfile.MaxRetractSpeedMMM = 25 * 60;
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