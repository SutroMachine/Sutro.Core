using g3;
using Sutro.Core.Assemblers;
using Sutro.Core.Logging;
using Sutro.Core.Models;
using Sutro.Core.Models.GCode;
using Sutro.Core.Parsers;
using Sutro.Core.Settings;
using Sutro.Core.Settings.Machine;
using Sutro.Core.Slicing;
using Sutro.Core.Utility;
using Sutro.Core.Writers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace Sutro.Core.Generators
{
    public class PrintGeneratorManager<TPrintGenerator, TPrintSettings> : IPrintGeneratorManager
        where TPrintGenerator : IPrintGenerator<TPrintSettings>, new()
        where TPrintSettings : class, IPrintProfileFFF, new()
    {
        public delegate ISettingsBuilder<TPrintSettings> SettingsBuilderF(TPrintSettings settings, ILogger logger);

        private readonly ILogger logger;
        private ISettingsBuilder<TPrintSettings> settingsBuilder;

        public bool AcceptsParts { get; } = true;

        public bool AcceptsPartSettings { get; } = false;

        public Version PrintGeneratorAssemblyVersion => Assembly.GetAssembly(typeof(TPrintGenerator)).GetName().Version;
        public string PrintGeneratorAssemblyName => Assembly.GetAssembly(typeof(TPrintGenerator)).GetName().Name;
        public string PrintGeneratorName => typeof(TPrintGenerator).Name;

        public ISettingsBuilder SettingsBuilder => settingsBuilder;

        public TPrintSettings Settings => settingsBuilder.Settings;

        public string Id { get; }
        public string Name { get; }
        public string Description { get; }

        public GCodeParserBase Parser { get; set; } = new GenericGCodeParser();
        public GCodeWriterBase Writer { get; set; } = new StandardGCodeWriter();

        protected static SettingsBuilderF DefaultSettingsBuilderF = (settings, logger) => new SettingsBuilder<TPrintSettings>(settings, logger);

        public PrintGeneratorManager(TPrintSettings settings, string id, string description, ILogger logger = null, bool acceptsParts = true,
            SettingsBuilderF settingsBuilderF = null)
        {
            AcceptsParts = acceptsParts;

            Id = id;
            Description = description;

            this.logger = logger ?? new NullLogger();

            settingsBuilder = (settingsBuilderF ?? DefaultSettingsBuilderF).Invoke(settings, logger);
        }

        public GenerationResult GCodeFromMesh(DMesh3 mesh,
            CancellationToken? cancellationToken = null)
        {
            return GCodeFromMesh(mesh, null, cancellationToken);
        }

        public GenerationResult GCodeFromMesh(DMesh3 mesh, TPrintSettings settings = null,
            CancellationToken? cancellationToken = null)
        {
            return GCodeFromMeshes(new DMesh3[] { mesh }, settings, cancellationToken);
        }

        public virtual GenerationResult GCodeFromMeshes(IEnumerable<DMesh3> meshes, TPrintSettings settings = null,
            CancellationToken? cancellationToken = null)
        {
            var printMeshAssembly = PrintMeshAssemblyFromMeshes(meshes);
            return GCodeFromPrintMeshAssembly(printMeshAssembly, settings);
        }

        public virtual GenerationResult GCodeFromPrintMeshAssembly(PrintMeshAssembly printMeshAssembly, TPrintSettings settings = null,
            CancellationToken? cancellationToken = null)
        {
            PlanarSliceStack slices = null;

            var globalSettings = settings ?? settingsBuilder.Settings;

            if (AcceptsParts)
            {
                var success = SliceMesh(printMeshAssembly, out slices);
                if (!success)
                {
                    var generationResult = new GenerationResult();
                    generationResult.AddLog(LoggingLevel.Error, "Mesh slicing failed");
                }
            }

            // Run the print generator
            logger.LogMessage("Running print generator...");
            var printGenerator = new TPrintGenerator();
            AssemblerFactoryF overrideAssemblerF = (globalSettings.MachineProfile as MachineProfileBase).AssemblerFactory();
            printGenerator.Initialize(printMeshAssembly, slices, globalSettings, overrideAssemblerF);

            return printGenerator.Generate(cancellationToken);
        }

        protected virtual PrintMeshAssembly PrintMeshAssemblyFromMeshes(IEnumerable<DMesh3> meshes)
        {
            if (AcceptsParts)
            {
                var printMeshAssembly = new PrintMeshAssembly();
                printMeshAssembly.AddMeshes(meshes, PrintMeshOptionsFactory.Default());
                return printMeshAssembly;
            }
            return null;
        }

        public Func<TPrintSettings, MeshPlanarSlicerBase> GetSlicerF { get; set; } =
            (settings) => new MeshSlicerHorizontalPlanes()
            {
                LayerHeightMM = settings.Part.LayerHeightMM
            };

        private bool SliceMesh(PrintMeshAssembly meshes, out PlanarSliceStack slices)
        {
            logger?.LogMessage("Slicing...");

            try
            {
                var slicer = GetSlicerF(Settings);

                slicer.Add(meshes);
                slices = slicer.Compute();
                return true;
            }
            catch (Exception e)
            {
                logger?.LogError(e.Message);
                if (Config.Debug)
                    throw;
                slices = null;
                return false;
            }
        }

        public virtual GCodeFile LoadGCode(TextReader input)
        {
            return Parser.Parse(input);
        }

        public virtual void SaveGCodeToFile(TextWriter output, GCodeFile file)
        {
            Writer.WriteFile(file, output);
        }
    }
}