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
            Machine.ManufacturerName = "RepRap";
            Machine.ModelIdentifier = "Generic";
            Machine.Class = MachineClass.PlasticFFFPrinter;
            Machine.BedSizeXMM = 80;
            Machine.BedSizeYMM = 80;
            Machine.OriginX = MachineBedOriginLocationX.Center;
            Machine.OriginY = MachineBedOriginLocationY.Center;

            Machine.MaxHeightMM = 55;
            Machine.NozzleDiamMM = 0.4;
            Material.FilamentDiamMM = 1.75;

            Machine.MaxExtruderTempC = 230;
            Machine.HasHeatedBed = false;
            Machine.MaxBedTempC = 60;

            Machine.MaxExtrudeSpeedMMM = 50 * 60;
            Machine.MaxTravelSpeedMMM = 150 * 60;
            Machine.MaxZTravelSpeedMMM = 100 * 60;
            Machine.MaxRetractSpeedMMM = 40 * 60;
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
    }
}