using Sutro.Core.Models.Profiles;

namespace gs
{
    public interface IPlanarAdditiveSettings : IProfile
    {
        double LayerHeightMM { get; }

        AssemblerFactoryF AssemblerType();

        MachineInfo BaseMachine { get; set; }
    }
}