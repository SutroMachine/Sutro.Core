using gs;
using Sutro.Core.Models.Profiles;
using System;

namespace Sutro.Core.Settings.Machine
{
    public class MachineProfileFFF : MachineProfileBase
    {
        public override int SchemaVersion => PrintProfileFFF.SchemaVersion;

        public FirmwareOptions Firmware { get; set; }

        // Printer Mechanics

        public double NozzleDiamMM = 0.4;
        public double MinLayerHeightMM = 0.05;
        public double MaxLayerHeightMM = 0.3;

        // Temperatures

        public int MinExtruderTempC = 20;
        public int MaxExtruderTempC = 230;

        public bool HasHeatedBed = false;
        public int MinBedTempC = 0;
        public int MaxBedTempC = 0;

        // Speed Limits
        // All units are mm/min = (mm/s * 60)

        public int MaxExtrudeSpeedMMM = 50 * 60;
        public int MaxTravelSpeedMMM = 100 * 60;
        public int MaxZTravelSpeedMMM = 20 * 60;
        public int MaxRetractSpeedMMM = 20 * 60;

        // Bed Leveling

        public bool HasAutoBedLeveling = false;
        public bool EnableAutoBedLeveling = false;

        // Hacks

        /// <summary>
        /// Avoid emitting gcode extrusion points closer than this spacing.
        /// </summary>
        /// <remarks>
        /// This is a workaround for the fact that many printers do not gracefully
        /// handle very tiny sequential extrusion steps. This setting could be
        /// configured using CalibrationModelGenerator.MakePrintStepSizeTest() with
        /// all other cleanup steps disabled.
        /// [TODO] this is actually speed-dependent...
        /// </remarks>
        public double MinPointSpacingMM = 0.1;

        public override IProfile Clone()
        {
            return SettingsPrototype.CloneAs<MachineProfileFFF, MachineProfileFFF>(this);
        }

        public override AssemblerFactoryF AssemblerFactory()
        {
            return Firmware switch
            {
                FirmwareOptions.RepRap => RepRapAssembler.Factory,
                FirmwareOptions.Prusa => PrusaAssembler.Factory,
                FirmwareOptions.Printrbot => PrintrbotAssembler.Factory,
                FirmwareOptions.Monoprice => RepRapAssembler.Factory,
                FirmwareOptions.Makerbot => MakerbotAssembler.Factory,
                FirmwareOptions.Flashforge => FlashforgeAssembler.Factory,
                _ => throw new NotImplementedException(),
            };
        }
    }
}