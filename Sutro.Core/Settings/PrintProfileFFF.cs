using gs;
using Sutro.Core.FunctionalTest;
using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings.Machine;
using Sutro.Core.Settings.Material;
using Sutro.Core.Settings.Part;
using System;

namespace Sutro.Core.Settings
{
    public class PrintProfileFFF : PrintProfileBase<
        MachineProfileFFF, MaterialProfileFFF, PartProfileFFF>
    {
        public static int SchemaVersion => 1;

        public override AssemblerFactoryF AssemblerFactory()
        {
            return MachineProfile.Firmware switch
            {
                FirmwareOptions.RepRap => RepRapAssembler.Factory,
                FirmwareOptions.Prusa => PrusaAssembler.Factory,
                FirmwareOptions.Printrbot => PrintrbotAssembler.Factory,
                FirmwareOptions.Monoprice => RepRapAssembler.Factory,
                FirmwareOptions.Makerbot => MakerbotAssembler.Factory,
                FirmwareOptions.Flashforge => FlashforgeAssembler.Factory,
                _ => throw new NotImplementedException(),
            };
        }

        public PrintProfileFFF()
        {
            FillTypeFactory = new FillTypeFactory(this);
            MachineProfile = new MachineProfileFFF();
            MaterialProfile = new MaterialProfileFFF();
            PartProfile = new PartProfileFFF();
        }

        public virtual IPrintProfile Clone()
        {
            return CloneAs<PrintProfileFFF>();
        }

        public FillTypeFactory FillTypeFactory { get; }

        public virtual double ShellsFillPathSpacingMM()
        {
            return MachineProfile.NozzleDiamMM * PartProfile.ShellsFillNozzleDiamStepX;
        }

        public virtual double SolidFillPathSpacingMM()
        {
            return MachineProfile.NozzleDiamMM * PartProfile.SolidFillNozzleDiamStepX;
        }

        public virtual double BridgeFillPathSpacingMM()
        {
            return MachineProfile.NozzleDiamMM * PartProfile.BridgeFillNozzleDiamStepX;
        }
    }
}