using g3;
using gs;
using Sutro.Core.Logging;
using Sutro.Core.Models.GCode;
using System.IO;

namespace Sutro.Core.FunctionalTest
{
    public class ResultGenerator<TGenerator, TSettings> : IResultGenerator
        where TGenerator : IPrintGenerator<TSettings>, new()
        where TSettings : PlanarAdditiveSettings, new()
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

        public GenerationResult GenerateResultFile(string meshFilePath, string outputFilePath)
        {
            var mesh = StandardMeshReader.ReadMesh(meshFilePath);
            var result = generator.GCodeFromMesh(mesh);
            SaveGCode(outputFilePath, result.GCode);
            return result;
        }
    }
}