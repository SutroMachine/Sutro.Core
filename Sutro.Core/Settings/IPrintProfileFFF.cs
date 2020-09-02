using gs;
using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings.Machine;
using Sutro.Core.Settings.Material;
using Sutro.Core.Settings.Part;

namespace Sutro.Core.Settings
{
    public interface IPrintProfileFFF : IPrintProfile, IPlanarAdditiveProfile
    {
        FillTypeFactory FillTypeFactory { get; }

        MachineProfileFFF Machine { get; set; }
        MaterialProfileFFF Material { get; set; }
        PartProfileFFF Part { get; set; }

        public double ShellsFillPathSpacingMM()
        {
            return Machine.NozzleDiamMM * Part.ShellsFillNozzleDiamStepX;
        }

        public double SolidFillPathSpacingMM()
        {
            return Machine.NozzleDiamMM * Part.SolidFillNozzleDiamStepX;
        }

        public double BridgeFillPathSpacingMM()
        {
            return Machine.NozzleDiamMM * Part.BridgeFillNozzleDiamStepX;
        }
    }
}