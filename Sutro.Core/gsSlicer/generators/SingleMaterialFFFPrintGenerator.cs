using Sutro.Core.Models.GCode;
using Sutro.Core.Settings;

namespace gs
{
    public class SingleMaterialFFFPrintGenerator : ThreeAxisPrintGenerator
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
            AssemblerFactoryF useAssembler = overrideAssemblerF ?? settings.AssemblerFactory();
            compiler = new SingleMaterialFFFCompiler(builder, settings, useAssembler);
            Initialize(meshes, slices, settings, compiler);
        }

        protected override GCodeFile extract_result()
        {
            return file_accumulator.File;
        }
    }
}