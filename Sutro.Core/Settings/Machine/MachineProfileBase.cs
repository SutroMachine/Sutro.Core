using gs;
using Sutro.Core.Models.Profiles;

namespace Sutro.Core.Settings.Machine
{
    public abstract class MachineProfileBase : IMachineProfile
    {
        public string Name { get; set; } = "Generic";

        public string ManufacturerName { get; set; } = "Unknown";
        public string ModelIdentifier { get; set; } = "Machine";

        public MachineClass Class = MachineClass.Unknown;

        public double BedSizeXMM { get; set; } = 100;
        public double BedSizeYMM { get; set; } = 100;
        public double MaxHeightMM { get; set; } = 100;

        public double BedSizeZMM { get => MaxHeightMM; set => MaxHeightMM = value; }

        public MachineBedOriginLocationX OriginX { get; set; }
        public MachineBedOriginLocationY OriginY { get; set; }

        public abstract int SchemaVersion { get; }

        public abstract AssemblerFactoryF AssemblerFactory();

        public abstract IProfile Clone();
    }
}