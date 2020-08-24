using gs;
using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings.Machine;

namespace Sutro.Core.Settings
{
    public interface IPlanarAdditiveSettings : IProfile
    {
        double LayerHeightMM { get; }

        AssemblerFactoryF AssemblerType();

        MachineInfo BaseMachine { get; set; }
    }
}