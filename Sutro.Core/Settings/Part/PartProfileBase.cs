using Sutro.Core.Models.Profiles;

namespace Sutro.Core.Settings.Part
{
    public abstract class PartProfileBase : SettingsPrototype, IPartProfile
    {
        public virtual double LayerHeightMM { get; set; } = 0.2;

        public virtual string Name { get; set; } = "Default";

        public abstract int SchemaVersion { get; }

        public abstract IProfile Clone();
    }
}