using Sutro.Core.Models.Profiles;

namespace gs
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