using Sutro.Core.FillTypes;
using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings.Machine;
using Sutro.Core.Settings.Material;
using Sutro.Core.Settings.Part;

namespace Sutro.Core.Settings
{
    public class PrintProfileFFF : PrintProfileBase<
        MachineProfileFFF, MaterialProfileFFF, PartProfileFFF>, IPrintProfileFFF
    {
        public static int SchemaVersion => 1;

        public PrintProfileFFF()
        {
            FillTypeFactory = new FillTypeFactory(this);
            Machine = new MachineProfileFFF();
            Material = new MaterialProfileFFF();
            Part = new PartProfileFFF();
        }

        public virtual IPrintProfile Clone()
        {
            return SettingsPrototype.CloneAs<PrintProfileFFF, PrintProfileFFF>(this);
        }

        IPlanarAdditiveProfile IPlanarAdditiveProfile.Clone()
        {
            return SettingsPrototype.CloneAs<PrintProfileFFF, PrintProfileFFF>(this);
        }

        public FillTypeFactory FillTypeFactory { get; }
    }

    public static class PrintProfileFFFExtensions
    {
        public static double ShellsFillPathSpacingMM(this IPrintProfileFFF profile)
        {
            return profile.Machine.NozzleDiamMM * profile.Part.ShellsFillNozzleDiamStepX;
        }

        public static double SolidFillPathSpacingMM(this IPrintProfileFFF profile)
        {
            return profile.Machine.NozzleDiamMM * profile.Part.SolidFillNozzleDiamStepX;
        }

        public static double BridgeFillPathSpacingMM(this IPrintProfileFFF profile)
        {
            return profile.Machine.NozzleDiamMM * profile.Part.BridgeFillNozzleDiamStepX;
        }
    }
}