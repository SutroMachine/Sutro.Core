using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings.Machine;
using System.Collections.Generic;

namespace Sutro.Core.Settings.Info
{
    public class RepRapSettings : GenericRepRapSettings
    {
        public override IPrintProfile Clone()
        {
            return CloneAs<RepRapSettings>();
        }

        public RepRapSettings() : base()
        {
            ConfigureUnknown();
        }

        public static RepRapSettings CreateGeneric()
        {
            return new RepRapSettings();
        }

        public static IEnumerable<PrintProfileFFF> EnumerateDefaults()
        {
            yield return CreateGeneric();
        }

        private void ConfigureUnknown()
        {
            MachineProfile.ManufacturerName = "RepRap";
            MachineProfile.ModelIdentifier = "Generic";
            MachineProfile.Class = MachineClass.PlasticFFFPrinter;
            MachineProfile.BedSizeXMM = 80;
            MachineProfile.BedSizeYMM = 80;
            MachineProfile.OriginX = MachineBedOriginLocationX.Center;
            MachineProfile.OriginY = MachineBedOriginLocationY.Center;

            MachineProfile.MaxHeightMM = 55;
            MachineProfile.NozzleDiamMM = 0.4;
            MaterialProfile.FilamentDiamMM = 1.75;

            MachineProfile.MaxExtruderTempC = 230;
            MachineProfile.HasHeatedBed = false;
            MachineProfile.MaxBedTempC = 60;

            MachineProfile.MaxExtrudeSpeedMMM = 50 * 60;
            MachineProfile.MaxTravelSpeedMMM = 150 * 60;
            MachineProfile.MaxZTravelSpeedMMM = 100 * 60;
            MachineProfile.MaxRetractSpeedMMM = 40 * 60;
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
    }
}