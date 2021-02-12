using Sutro.Core.Compilers;
using Sutro.Core.Settings;

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
    }
}