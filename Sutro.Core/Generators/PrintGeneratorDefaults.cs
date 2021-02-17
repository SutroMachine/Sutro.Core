using g3;
using Sutro.Core.Compilers;
using Sutro.Core.PartExteriors;
using Sutro.Core.Settings;
using Sutro.Core.Settings.Part;
using Sutro.Core.Slicing;
using System;

namespace Sutro.Core.Generators
{
    /// <summary>
    /// Default implementations of "pluggable" ThreeAxisPrintGenerator functions
    /// </summary>
    public static class PrintGeneratorDefaults
    {
        /*
         * Compiler Post-Processors
         */

        public static void AppendPrintStatistics<T>(
            IThreeAxisPrinterCompiler compiler, ThreeAxisPrintGenerator<T> printgen) where T : IPrintProfileFFF
        {
            compiler.AppendComment("".PadRight(79, '-'));
            foreach (string line in printgen.TotalPrintTimeStatistics.ToStringList())
            {
                compiler.AppendComment(" " + line);
            }
            compiler.AppendComment("".PadRight(79, '-'));
            foreach (string line in printgen.TotalExtrusionReport)
            {
                compiler.AppendComment(" " + line);
            }
            compiler.AppendComment("".PadRight(79, '-'));
        }

        public static IPartExterior PartExteriorFactory(PlanarSliceStack sliceStack, IPrintProfileFFF profile)
        {
            double minArea = Math.Pow(profile.Machine.NozzleDiamMM, 2);

            // TODO: Make configurable
            double anchorDistance = profile.Machine.NozzleDiamMM * 2;

            return new PartExteriorVerticalProjection(sliceStack,
                minArea, anchorDistance,
                profile.Part.FloorLayers, profile.Part.RoofLayers);
        }
    }
}