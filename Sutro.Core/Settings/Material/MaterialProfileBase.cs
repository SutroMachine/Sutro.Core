using Sutro.Core.Models.Profiles;

namespace Sutro.Core.Settings.Material
{
    public abstract class MaterialProfileBase : SettingsPrototype, IMaterialProfile
    {
        protected string name = null;

        // Identifiers
        public string Material { get; set; } = "PLA";
        public string Supplier { get; set; } = "Generic";

        public virtual string Name
        {
            get => name ?? $"{Supplier} {Material}";
            set { name = value; }
        }

        // Properties

        public double CostPerKG { get; set; } = 19.19;
        public double GramsPerCubicMM { get; set; } = 0.00125;

        public abstract int SchemaVersion { get; }
        public virtual string MaterialName { get => Name; set => Name = value; }

        public abstract IProfile Clone();
    }
}
