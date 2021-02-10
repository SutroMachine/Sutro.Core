using Sutro.Core.Models.Profiles;

namespace Sutro.Core.Settings.Material
{
    public class MaterialProfileFFF : MaterialProfileBase
    {
        // Identifiers

        public string Color { get; set; } = "Blue";

        // Properties

        public double FilamentDiamMM { get; set; } = 1.75;

        // Temperatures

        public int ExtruderTempC { get; set; } = 210;
        public int HeatedBedTempC { get; set; } = 0;
        public override int SchemaVersion => PrintProfileFFF.SchemaVersion;

        public override IProfile Clone()
        {
            return SettingsPrototype.CloneAs<MaterialProfileFFF, MaterialProfileFFF>(this);
        }
    }
}