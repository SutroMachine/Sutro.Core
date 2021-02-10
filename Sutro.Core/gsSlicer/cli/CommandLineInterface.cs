using CommandLine;
using CommandLine.Text;
using g3;
using Sutro.Core;
using Sutro.Core.Logging;
using Sutro.Core.Models.GCode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace gs
{
    public class CommandLineInterface
    {
        protected readonly ILogger logger;
        protected readonly Dictionary<string, IPrintGeneratorManager> printGeneratorDict;

        protected IPrintGeneratorManager printGeneratorManager;

        public CommandLineInterface(ILogger logger, IEnumerable<IPrintGeneratorManager> printGenerators)
        {
            this.logger = logger;
            printGeneratorDict = new Dictionary<string, IPrintGeneratorManager>();
            foreach (var printGenerator in printGenerators)
                printGeneratorDict.Add(printGenerator.Id, printGenerator);
        }

        public void Execute(string[] args)
        {
            var parser = new Parser(with => with.HelpWriter = null);

            var parserResult = parser.ParseArguments<CommandLineOptions>(args);

            parserResult.WithParsed(ParsingSuccessful);

            parserResult.WithNotParsed((err) => ParsingUnsuccessful(err, parserResult));
        }

        protected static bool OutputFilePathIsValid(CommandLineOptions o)
        {
            if (o.GCodeFilePath is null || !Directory.Exists(Directory.GetParent(o.GCodeFilePath).ToString()))
            {
                Console.WriteLine("Must provide valid gcode file path as second argument.");
                return false;
            }
            return true;
        }

        protected virtual void CenterMeshAboveOrigin(DMesh3 mesh)
        {
            MeshTransforms.Translate(mesh, new Vector3d(-mesh.CachedBounds.Center.x, -mesh.CachedBounds.Center.y, 0));
        }

        protected virtual void ConsoleWriteSeparator()
        {
            logger.WriteLine("".PadRight(79, '-'));
        }

        protected virtual bool ConstructSettings(CommandLineOptions o)
        {
            foreach (var file in o.SettingsFiles)
            {
                try
                {
                    printGeneratorManager.SettingsBuilder.ApplyJSONFile(file);
                }
                catch (Exception e)
                {
                    logger.WriteLine(e.Message);
                    return false;
                }
            }

            foreach (var snippet in o.SettingsOverride)
            {
                try
                {
                    printGeneratorManager.SettingsBuilder.ApplyJSONSnippet(snippet);
                }
                catch (Exception e)
                {
                    logger.WriteLine(e.Message);
                    return false;
                }
            }
            return true;
        }

        protected virtual void DropMeshToBuildPlate(DMesh3 mesh)
        {
            MeshTransforms.Translate(mesh, new Vector3d(0, 0, mesh.CachedBounds.Extents.z - mesh.CachedBounds.Center.z));
        }

        protected GenerationResult GenerateGCode(DMesh3 mesh)
        {
            ConsoleWriteSeparator();
            logger.WriteLine($"GENERATION");
            logger.WriteLine();
            return printGeneratorManager.GCodeFromMesh(mesh, null);
        }

        protected virtual void LoadMesh(CommandLineOptions o, out DMesh3 mesh)
        {
            if (printGeneratorManager.AcceptsParts)
            {
                string fMeshFilePath = Path.GetFullPath(o.MeshFilePath);
                ConsoleWriteSeparator();
                logger.WriteLine($"PARTS");
                logger.WriteLine();

                logger.Write("Loading mesh " + fMeshFilePath + "...");
                mesh = StandardMeshReader.ReadMesh(fMeshFilePath);
                logger.WriteLine(" done.");

                logger.Write("Repairing mesh... ");
                bool repaired = new MeshAutoRepair(mesh).Apply();
                logger.WriteLine(repaired ? "repaired." : "not repaired.");

                if (o.CenterXY) CenterMeshAboveOrigin(mesh);
                if (o.DropZ) DropMeshToBuildPlate(mesh);
            }
            else
            {
                mesh = null;
            }
        }

        protected virtual bool MeshFilePathIsValid(CommandLineOptions o)
        {
            if (printGeneratorManager.AcceptsParts && (o.MeshFilePath is null || !File.Exists(o.MeshFilePath)))
            {
                Console.WriteLine("Must provide valid mesh file path as third argument.");
                Console.WriteLine(Path.GetFullPath(o.MeshFilePath));
                return false;
            }
            return true;
        }

        protected virtual void OutputGenerationDetails(GCodeGenerationDetails generationDetails)
        {
            ConsoleWriteSeparator();

            WriteStringCollection("TOTAL EXTRUSION ESTIMATE:", generationDetails.MaterialUsageEstimate);

            WriteStringCollection("TOTAL PRINT TIME ESTIMATE:", generationDetails.PrintTimeEstimate);

            WriteStringCollection("WARNINGS:", generationDetails.Warnings, ConsoleColor.Red);

            logger.WriteLine("Print generation complete.");
        }

        private void WriteStringCollection(string label, IReadOnlyCollection<string> lines, ConsoleColor? color = null)
        {
            if (lines == null || lines.Count == 0)
                return;

            logger.WriteLine(label.ToUpper(), color);

            foreach (var line in lines)
            {
                logger.WriteLine(line, color);
            }

            logger.WriteLine();
        }

        protected virtual void OutputLog(GenerationResult result, int verbosity)
        {
            var errors = result.LogEntries.Where(l => l.Level == LoggingLevel.Error).ToList();
            var warnings = result.LogEntries.Where(l => l.Level == LoggingLevel.Warning).ToList();
            var info = result.LogEntries.Where(l => l.Level == LoggingLevel.Info).ToList();

            ConsoleWriteSeparator();
            logger.WriteLine("GENERATION LOG");
            logger.WriteLine(string.Empty);
            logger.WriteLine($"Print generated with {errors.Count} errors and {warnings.Count} warnings.");
            logger.WriteLine("");
            foreach (var logEntry in result.LogEntries)
            {
                OutputLogEntry(logEntry, verbosity);
            }
        }

        protected virtual void OutputLogEntry(LogEntry logEntry, int verbosity)
        {
            if (logEntry.Level == LoggingLevel.Error)
            {
                logger.WriteLine($"error: {logEntry.Message}", ConsoleColor.Red);
            }
            else if (logEntry.Level == LoggingLevel.Warning && verbosity >= 1)
            {
                logger.WriteLine($"warning: {logEntry.Message}", ConsoleColor.Yellow);
            }
            else if (logEntry.Level == LoggingLevel.Warning && verbosity >= 2)
            {
                logger.WriteLine($"warning: {logEntry.Message}", ConsoleColor.Gray);
            }
        }

        protected virtual void OutputVersionInfo()
        {
            ConsoleWriteSeparator();
            var version = printGeneratorManager.PrintGeneratorAssemblyVersion;
            logger.WriteLine($"Using {printGeneratorManager.PrintGeneratorName} from {printGeneratorManager.PrintGeneratorAssemblyName} v{version.Major}.{version.Minor}.{version.Revision}");
            logger.WriteLine();
        }

        protected void ParsingSuccessful(CommandLineOptions o)
        {
            if (!printGeneratorDict.TryGetValue(o.Generator, out printGeneratorManager))
            {
                HandleInvalidGeneratorId(o.Generator);
                return;
            }

            OutputVersionInfo();

            if (!MeshFilePathIsValid(o)) return;

            if (!OutputFilePathIsValid(o)) return;

            if (!ConstructSettings(o)) return;

            LoadMesh(o, out var mesh);

            var result = GenerateGCode(mesh);

            switch (result.Status)
            {
                default:
                case GenerationResultStatus.Failure:
                    logger.WriteLine("Print generation failed.", ConsoleColor.Red);
                    break;
                case GenerationResultStatus.Canceled:
                    logger.WriteLine("Print generation canceled.", ConsoleColor.Red);
                    break;
                case GenerationResultStatus.Success:
                    if (result.Status == GenerationResultStatus.Success)
                    {
                        logger.WriteLine("Print generation succeeded.", ConsoleColor.Green);
                        WriteGCodeToFile(o.GCodeFilePath, result.GCode);
                    }
                    break;
            }

            OutputLog(result, o.Verbosity);

            if (result.Status == GenerationResultStatus.Success)
                OutputGenerationDetails(result.Details);
        }


        protected virtual void HandleInvalidGeneratorId(string id)
        {
            logger.WriteLine($"Invalid generator id: {id}");
            logger.WriteLine();

            logger.WriteLine("Available generators:");
            ListAvailableGenerators();
            logger.WriteLine();
        }

        private void ListAvailableGenerators()
        {
            foreach (var g in printGeneratorDict.Values)
            {
                logger.Write($"{g.Id} ", ConsoleColor.Green);
                logger.Write($"{g.PrintGeneratorName} ", ConsoleColor.Yellow);
                logger.Write($"{g.Description}", ConsoleColor.Gray);
                logger.WriteLine();
            }
        }

        protected virtual void ParsingUnsuccessful(IEnumerable<Error> errs, ParserResult<CommandLineOptions> parserResult)
        {
            logger.WriteLine("ERRORS:");
            foreach (var err in errs)
                logger.WriteLine(err);
            logger.WriteLine();

            logger.WriteLine("HELP:");
            var helpText = HelpText.AutoBuild(parserResult, h => h, e => e);
            logger.WriteLine(helpText.ToString());
            logger.WriteLine();

            logger.WriteLine("GENERATORS:");
            ListAvailableGenerators();
            logger.WriteLine();
        }

        protected virtual string VersionToString(Version v)
        {
            return $"v{v.Major}.{v.Minor}.{v.Revision}";
        }

        protected virtual void WriteGCodeToFile(string filePath, GCodeFile gcode)
        {
            string gcodeFilePath = Path.GetFullPath(filePath);
            logger.WriteLine($"Writing gcode to {gcodeFilePath}");
            using (StreamWriter w = new StreamWriter(gcodeFilePath))
            {
                printGeneratorManager.SaveGCodeToFile(w, gcode);
            }
        }
    }
}