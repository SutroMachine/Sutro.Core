using Sutro.Core.Assemblers;
using Sutro.Core.Compilers;
using Sutro.Core.GCodeBuilders;
using Sutro.Core.Models.GCode;
using Sutro.Core.Settings;
using Sutro.Core.Slicing;

namespace Sutro.Core.Generators
{
    public class SingleMaterialFFFPrintGenerator : ThreeAxisPrintGenerator<PrintProfileFFF>
    {
        private GCodeFileAccumulator file_accumulator;
        private GCodeBuilder builder;
        private SingleMaterialFFFCompiler compiler;

        public SingleMaterialFFFPrintGenerator()
        {
        }

        public SingleMaterialFFFPrintGenerator(PrintMeshAssembly meshes,
                                               PlanarSliceStack slices,
                                               PrintProfileFFF settings,
                                               AssemblerFactoryF overrideAssemblerF = null)
        {
            Initialize(meshes, slices, settings, overrideAssemblerF);
        }

        public override void Initialize(PrintMeshAssembly meshes,
                               PlanarSliceStack slices,
                               PrintProfileFFF settings,
                               AssemblerFactoryF overrideAssemblerF = null)
        {
            file_accumulator = new GCodeFileAccumulator();
            builder = new GCodeBuilder(file_accumulator);
            AssemblerFactoryF useAssembler = overrideAssemblerF ?? settings.Machine.AssemblerFactory();
            compiler = new SingleMaterialFFFCompiler(builder, settings, useAssembler);
            Initialize(meshes, slices, settings, compiler);
        }

        protected override GCodeFile extract_result()
        {
            return file_accumulator.File;
        }
    }
}