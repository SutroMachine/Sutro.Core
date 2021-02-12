using g3;
using Sutro.Core.Generators;
using Sutro.Core.Logging;
using Sutro.Core.Models.GCode;
using Sutro.Core.Settings;
using System.IO;

namespace Sutro.Core.Test
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
            logger.LogMessage($"Saving file to {path}");
            using var streamWriter = new StreamWriter(path);
            generator.SaveGCodeToFile(streamWriter, gcode);
        }

        public GenerationResult GenerateResultFile(string meshFilePath, string outputFilePath)
        {
            var mesh = StandardMeshReader.ReadMesh(meshFilePath);

            var result = generator.GCodeFromMesh(
                mesh: mesh,
                cancellationToken: null);

            SaveGCode(outputFilePath, result.GCode);

            return result;
        }
    }
}