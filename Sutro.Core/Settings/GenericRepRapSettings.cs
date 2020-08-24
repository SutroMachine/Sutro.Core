using gs;
using Sutro.Core.Models.Profiles;

namespace Sutro.Core.Settings
{
    public class GenericRepRapSettings : SingleMaterialFFFSettings
    {
        public override AssemblerFactoryF AssemblerType()
        {
            return RepRapAssembler.Factory;
        }

        public override IProfile Clone()
        {
            return CloneAs<GenericRepRapSettings>();
        }
    }
}