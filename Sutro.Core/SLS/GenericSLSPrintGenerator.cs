using Sutro.Core.GCodeBuilders;
using Sutro.Core.Generators;
using Sutro.Core.Settings;
using Sutro.Core.Slicing;
using Sutro.Core.Toolpaths;

namespace Sutro.Core.SLS
{
    public class GenericSLSPrintGenerator : SLSPrintGenerator
    {
        private GCodeFileAccumulator file_accumulator;

        //GCodeBuilder builder;
        private SLSCompiler compiler;

        public GenericSLSPrintGenerator(PrintMeshAssembly meshes,
                                      PlanarSliceStack slices,
                                      PrintProfileFFF settings)
        {
            file_accumulator = new GCodeFileAccumulator();
            //builder = new GCodeBuilder(file_accumulator);
            //compiler = new SLSCompiler(builder, settings);
            compiler = new SLSCompiler(settings);

            Initialize(meshes, slices, settings, compiler);
        }

        protected override ToolpathSet extract_result()
        {
            return compiler.TempGetAssembledPaths();
        }
    }
}