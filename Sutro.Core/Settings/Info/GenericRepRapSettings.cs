using Sutro.Core.Models.Profiles;

namespace Sutro.Core.Settings.Info
{
    public class GenericRepRapSettings : PrintProfileFFF
    {
        public GenericRepRapSettings()
        {
            MachineProfile.Firmware = Settings.Machine.FirmwareOptions.RepRap;
        }

        public override IPrintProfile Clone()
        {
            return CloneAs<GenericRepRapSettings>();
        }
    }
}