using gs;
using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings.Machine;
using Sutro.Core.Settings.Material;
using Sutro.Core.Settings.Part;

namespace Sutro.Core.Settings
{
    public abstract class PrintProfileBase : SettingsPrototype, IPrintProfile
    {
        public abstract IMachineProfile Machine { get; }
        public abstract IMaterialProfile Material { get; }
        public abstract IPartProfile Part { get; }
        public abstract double LayerHeightMM { get; set; }

        public abstract AssemblerFactoryF AssemblerFactory();
    }

    public abstract class PrintProfileBase<TMachine, TMaterial, TPart> : PrintProfileBase
        where TMachine : MachineProfileBase
        where TMaterial : MaterialProfileBase
        where TPart : PartProfileBase
    {
        public TMachine MachineProfile { get; set; }
        public TMaterial MaterialProfile { get; set; }
        public TPart PartProfile { get; set; }

        public override IMachineProfile Machine => MachineProfile;
        public override IMaterialProfile Material => MaterialProfile;
        public override IPartProfile Part => PartProfile;

        public override double LayerHeightMM
        {
            get => PartProfile.LayerHeightMM;
            set => PartProfile.LayerHeightMM = value;
        }
    }
}