using CommandLine;
using CommandLine.Text;
using g3;
using gs;
using Sutro.Core.Generators;
using Sutro.Core.Logging;
using Sutro.Core.Models.GCode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sutro.Core.CommandLine
{
    public class Interface
    {
        protected readonly ILogger logger;
        protected readonly Dictionary<string, IPrintGeneratorManager> printGeneratorDict;

        protected IPrintGeneratorManager printGeneratorManager;

        public Interface(ILogger logger, IEnumerable<IPrintGeneratorManager> printGenerators)
        {
            this.logger = logger;
            printGeneratorDict = new Dictionary<string, IPrintGeneratorManager>();
            foreach (var printGenerator in printGenerators)
                printGeneratorDict.Add(printGenerator.Id, printGenerator);
        }

        public void Execute(string[] args)
        {
            var parser = new Parser(with => with.HelpWriter = null);

            var parserResult = parser.ParseArguments<Options>(args);

            parserResult.WithParsed(ParsingSuccessful);

            parserResult.WithNotParsed((err) => ParsingUnsuccessful(err, parserResult));
        }

        protected bool OutputFilePathIsValid(Options o)
        {
            if (o.GCodeFilePath is null || !Directory.Exists(Directory.GetParent(o.GCodeFilePath).ToString()))
            {
                logger.LogError("Must provide valid gcode file path as second argument.");
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
            logger.LogMessage("".PadRight(79, '-'));
        }

        protected virtual bool ConstructSettings(Options o)
        {
            foreach (var file in o.SettingsFiles)
            {
                try
                {
                    printGeneratorManager.SettingsBuilder.ApplyJSONFile(file);
                }
                catch (Exception e)
                {
                    logger.LogError(e.Message);
                    if (Models.Config.Debug) throw;
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
                    logger.LogError(e.Message);
                    if (Models.Config.Debug) throw;
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
            logger.LogMessage($"GENERATION");
            logger.LogMessage(string.Empty);
            return printGeneratorManager.GCodeFromMesh(mesh, null);
        }

        protected virtual void LoadMesh(Options o, out DMesh3 mesh)
        {
            if (printGeneratorManager.AcceptsParts)
            {
                string fMeshFilePath = Path.GetFullPath(o.MeshFilePath);
                ConsoleWriteSeparator();
                logger.LogMessage($"PARTS");
                logger.LogMessage(string.Empty);

                logger.LogMessage("Loading mesh " + fMeshFilePath + "...");
                mesh = StandardMeshReader.ReadMesh(fMeshFilePath);

                if (o.Repair)
                {
                    logger.LogMessage("Repairing mesh... ");
                    bool repaired = new MeshAutoRepair(mesh).Apply();
                    logger.LogMessage(repaired ? "Mesh repaired." : "Mesh not repaired.");
                }

                if (o.CenterXY) CenterMeshAboveOrigin(mesh);
                if (o.DropZ) DropMeshToBuildPlate(mesh);
            }
            else
            {
                mesh = null;
            }
        }

        protected virtual bool MeshFilePathIsValid(Options o)
        {
            if (printGeneratorManager.AcceptsParts && (o.MeshFilePath is null || !File.Exists(o.MeshFilePath)))
            {
                logger.LogError("Must provide valid mesh file path as third argument.");
                logger.LogError(Path.GetFullPath(o.MeshFilePath));
                return false;
            }
            return true;
        }

        protected virtual void OutputGenerationDetails(GCodeGenerationDetails generationDetails)
        {
            ConsoleWriteSeparator();

            WriteStringCollection("TOTAL EXTRUSION ESTIMATE:", generationDetails.MaterialUsageEstimate);

            WriteStringCollection("TOTAL PRINT TIME ESTIMATE:", generationDetails.PrintTimeEstimate);

            WriteStringCollection("WARNINGS:", generationDetails.Warnings);

            logger.LogMessage("Print generation complete.");
        }

        private void WriteStringCollection(string label, IReadOnlyCollection<string> lines)
        {
            if (lines == null || lines.Count == 0)
                return;

            logger.LogMessage(label.ToUpper());

            foreach (var line in lines)
            {
                logger.LogMessage(line);
            }

            logger.LogMessage(string.Empty);
        }

        protected virtual void OutputLog(GenerationResult result, int verbosity)
        {
            var errors = result.LogEntries.Where(l => l.Level == LoggingLevel.Error).ToList();
            var warnings = result.LogEntries.Where(l => l.Level == LoggingLevel.Warning).ToList();
            var info = result.LogEntries.Where(l => l.Level == LoggingLevel.Info).ToList();

            ConsoleWriteSeparator();
            logger.LogMessage("GENERATION LOG");
            logger.LogMessage(string.Empty);
            logger.LogMessage($"Print generated with {errors.Count} errors and {warnings.Count} warnings.");
            logger.LogMessage(string.Empty);
            foreach (var logEntry in result.LogEntries)
            {
                OutputLogEntry(logEntry, verbosity);
            }
        }

        protected virtual void OutputLogEntry(LogEntry logEntry, int verbosity)
        {
            if (logEntry.Level == LoggingLevel.Error)
            {
                logger.LogError(logEntry.Message);
            }
            else if (logEntry.Level == LoggingLevel.Warning && verbosity >= 1)
            {
                logger.LogWarning(logEntry.Message);
            }
            else if (logEntry.Level == LoggingLevel.Info && verbosity >= 2)
            {
                logger.LogInfo(logEntry.Message);
            }
        }

        protected virtual void OutputVersionInfo()
        {
            ConsoleWriteSeparator();
            var version = printGeneratorManager.PrintGeneratorAssemblyVersion;
            logger.LogMessage($"Using {printGeneratorManager.PrintGeneratorName} from {printGeneratorManager.PrintGeneratorAssemblyName} v{version.Major}.{version.Minor}.{version.Revision}");
            logger.LogMessage(string.Empty);
        }

        protected void ParsingSuccessful(Options o)
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
                    logger.LogError("Print generation failed.");
                    break;

                case GenerationResultStatus.Canceled:
                    logger.LogError("Print generation canceled.");
                    break;

                case GenerationResultStatus.Success:
                    if (result.Status == GenerationResultStatus.Success)
                    {
                        logger.LogMessage("Print generation succeeded.");
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
            logger.LogMessage($"Invalid generator id: {id}");
            logger.LogMessage(string.Empty);

            logger.LogMessage("Available generators:");
            ListAvailableGenerators();
            logger.LogMessage(string.Empty);
        }

        private void ListAvailableGenerators()
        {
            foreach (var g in printGeneratorDict.Values)
            {
                logger.LogMessage($"{g.Id} {g.PrintGeneratorName} {g.Description}");
                logger.LogMessage(string.Empty);
            }
        }

        protected virtual void ParsingUnsuccessful(IEnumerable<Error> errs, ParserResult<Options> parserResult)
        {
            logger.LogMessage("ERRORS:");
            foreach (var err in errs)
                logger.LogMessage(err.ToString());
            logger.LogMessage(string.Empty);

            logger.LogMessage("HELP:");
            var helpText = HelpText.AutoBuild(parserResult, h => h, e => e);
            logger.LogMessage(helpText.ToString());
            logger.LogMessage(string.Empty);

            logger.LogMessage("GENERATORS:");
            ListAvailableGenerators();
            logger.LogMessage(string.Empty);
        }

        protected virtual string VersionToString(Version v)
        {
            return $"v{v.Major}.{v.Minor}.{v.Revision}";
        }

        protected virtual void WriteGCodeToFile(string filePath, GCodeFile gcode)
        {
            string gcodeFilePath = Path.GetFullPath(filePath);
            logger.LogMessage($"Writing gcode to {gcodeFilePath}");
            using (StreamWriter w = new StreamWriter(gcodeFilePath))
            {
                printGeneratorManager.SaveGCodeToFile(w, gcode);
            }
        }
    }
}