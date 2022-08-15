using g3;
using gs;
using Sutro.Core.Models.GCode;
using Sutro.Core.Settings;
using System.IO;

namespace Sutro.Core.FunctionalTest
{
    public class ResultGenerator<TGenerator, TSettings> : IResultGenerator
        where TGenerator : IPrintGenerator<TSettings>, new()
        where TSettings : class, IPrintProfileFFF, new()
    {
        private readonly PrintGeneratorManager<TGenerator, TSettings> generator;
        private readonly ILogger logger;

        public ResultGenerator(PrintGeneratorManager<TGenerator, TSettings> generator, ILogger logger)
        {
            this.generator = generator;
            this.logger = logger;
        }

        protected void SaveGCode(string path, GCodeFile gcode)
        {
            logger.WriteLine($"Saving file to {path}");
            using var streamWriter = new StreamWriter(path);
            generator.SaveGCodeToFile(streamWriter, gcode);
        }

        public void GenerateResultFile(string meshFilePath, string outputFilePath)
        {
            var mesh = StandardMeshReader.ReadMesh(meshFilePath);

            MeshTransforms.Translate(mesh, new Vector3d(0, 0, mesh.CachedBounds.Extents.z - mesh.CachedBounds.Center.z));

            var gcode = generator.GCodeFromMesh(
                mesh: mesh, 
                details: out _,
                cancellationToken: null);

            SaveGCode(outputFilePath, gcode);
        }
    }
}