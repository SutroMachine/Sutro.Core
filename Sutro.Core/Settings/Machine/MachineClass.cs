namespace Sutro.Core.Settings.Machine
{
    public enum MachineClass
    {
        // IMPORTANT: Do not change the order of existing enum values, otherwise
        // compatability of serialized objects may be inconsistent

        Unknown,
        PlasticFFFPrinter,
        MetalSLSPrinter,
        MetalDODPrinter,
        MetalEBMPrinter,
        MetalLMDPrinter,
        MetalMPAPrinter,
        PlasticSLAPrinter,
        PlasticDLPPrinter,
        PlasticSLSPrinter
    }
}