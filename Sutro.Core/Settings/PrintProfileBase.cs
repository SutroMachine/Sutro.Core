using gs;
using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings.Machine;
using Sutro.Core.Settings.Material;
using Sutro.Core.Settings.Part;

namespace Sutro.Core.Settings
{
    public abstract class PrintProfileBase : IPrintProfile
    {
        public abstract IMachineProfile MachineProfile { get; }
        public abstract IMaterialProfile MaterialProfile { get; }
        public abstract IPartProfile PartProfile { get; }
        public abstract double LayerHeightMM { get; set; }
    }

    public abstract class PrintProfileBase<TMachine, TMaterial, TPart> : PrintProfileBase
        where TMachine : MachineProfileBase
        where TMaterial : MaterialProfileBase
        where TPart : PartProfileBase
    {
        public TMachine Machine { get; set; }
        public TMaterial Material { get; set; }
        public TPart Part { get; set; }

        public override IMachineProfile MachineProfile => Machine;
        public override IMaterialProfile MaterialProfile => Material;
        public override IPartProfile PartProfile => Part;

        public override double LayerHeightMM
        {
            get => Part.LayerHeightMM;
            set => Part.LayerHeightMM = value;
        }
    }
}