using g3;
using gs.FillTypes;
using Sutro.Core;
using Sutro.Core.Logging;
using Sutro.Core.Models;
using Sutro.Core.Models.GCode;
using Sutro.Core.Models.Profiles;
using Sutro.Core.Settings;
using Sutro.Core.Settings.Machine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;

namespace gs
{
    public class PrintGeneratorManager<TPrintGenerator, TPrintSettings> : IPrintGeneratorManager
        where TPrintGenerator : IPrintGenerator<TPrintSettings>, new()
        where TPrintSettings : class, IPrintProfileFFF, new()
    {
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

        public PrintGeneratorManager(TPrintSettings settings, string id, string description, ILogger logger = null, bool acceptsParts = true)
        {
            AcceptsParts = acceptsParts;

            Id = id;
            Description = description;

            settingsBuilder = new SettingsBuilder<TPrintSettings>(settings, logger);
            this.logger = logger ?? new NullLogger();
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
                SliceMesh(printMeshAssembly, out slices, globalSettings.Part.LayerHeightMM);
            }

            // Run the print generator
            logger.WriteLine("Running print generator...");
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

        protected virtual void SliceMesh(PrintMeshAssembly meshes, out PlanarSliceStack slices, double layerHeight)
        {
            logger?.WriteLine("Slicing...");

            // Do slicing
            MeshPlanarSlicer slicer = new MeshPlanarSlicer()
            {
                LayerHeightMM = layerHeight,
            };

            slicer.Add(meshes);
            slices = slicer.Compute();
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