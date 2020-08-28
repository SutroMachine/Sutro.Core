using Sutro.Core.Models.Profiles;

namespace Sutro.Core.Settings.Material
{

    public class MaterialProfileFFF : MaterialProfileBase
    {
        // Identifiers

        public string Color { get; set; } = "Blue";

        public override string MaterialName
        {
            get => name ?? $"{Supplier} {Material} - {Color}";
            set { name = value; }
        }

        // Properties

        public double FilamentDiamMM { get; set; } = 1.75;

        // Temperatures

        public int ExtruderTempC { get; set; } = 210;
        public int HeatedBedTempC { get; set; } = 0;
        public override int SchemaVersion => PrintProfileFFF.SchemaVersion;

        public override IProfile Clone()
        {
            return CloneAs<MaterialProfileFFF>();
        }
    }
}
