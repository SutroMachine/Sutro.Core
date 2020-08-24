using Sutro.Core.Settings;

namespace Sutro.Core.Settings.Machine
{
    public abstract class MachineInfo : SettingsPrototype
    {
        protected readonly static string UnknownUUID = "00000000-0000-0000-0000-000000000000";

        public string ManufacturerName = "Unknown";
        public string ManufacturerUUID = UnknownUUID;
        public string ModelIdentifier = "Machine";
        public string ModelUUID = UnknownUUID;
        public MachineClass Class = MachineClass.Unknown;

        public double BedSizeXMM = 100;
        public double BedSizeYMM = 100;
        public double MaxHeightMM = 100;

        public double BedOriginFactorX = 0;
        public double BedOriginFactorY = 0;
    }
}