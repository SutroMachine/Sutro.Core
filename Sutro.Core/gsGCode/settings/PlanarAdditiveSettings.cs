using Sutro.Core.Models;
using Sutro.Core.Models.Profiles;

namespace gs
{
    public abstract class PlanarAdditiveSettings : SettingsPrototype, IPlanarAdditiveSettings
    {
        /// <summary>
        /// This is the "name" of this settings (eg user identifier)
        /// </summary>
        public string Identifier = "Defaults";

        public double LayerHeightMM { get; set; } = 0.2;

        public abstract MachineInfo BaseMachine { get; set; }

        public string ManufacturerName { get => BaseMachine.ManufacturerName; set => BaseMachine.ManufacturerName = value; }
        public string ModelIdentifier { get => BaseMachine.ModelIdentifier; set => BaseMachine.ModelIdentifier = value; }
        public double MachineBedSizeXMM { get => BaseMachine.BedSizeXMM; set => BaseMachine.BedSizeXMM = value; }
        public double MachineBedSizeYMM { get => BaseMachine.BedSizeYMM; set => BaseMachine.BedSizeYMM = value; }
        public double MachineBedSizeZMM { get => BaseMachine.MaxHeightMM; set => BaseMachine.MaxHeightMM = value; }

        public MachineBedOriginLocationX OriginX 
        {
            get => MachineBedOriginLocationUtility.LocationXFromScalar(BaseMachine.BedOriginFactorX);
            set => BaseMachine.BedOriginFactorX = MachineBedOriginLocationUtility.LocationXFromEnum(value);
        }

        public MachineBedOriginLocationY OriginY
        {
            get => MachineBedOriginLocationUtility.LocationYFromScalar(BaseMachine.BedOriginFactorY);
            set => BaseMachine.BedOriginFactorY = MachineBedOriginLocationUtility.LocationYFromEnum(value);
        }

        public abstract string MaterialName { get; set; }
        public abstract string ProfileName { get; set; }

        public abstract AssemblerFactoryF AssemblerType();

        public abstract IProfile Clone();
    }
}