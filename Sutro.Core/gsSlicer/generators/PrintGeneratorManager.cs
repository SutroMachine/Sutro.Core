using g3;
using Sutro.Core;
using Sutro.Core.Logging;
using Sutro.Core.Models;
using Sutro.Core.Models.GCode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace gs
{

    public class PrintGeneratorManager<TPrintGenerator, TPrintSettings> : IPrintGeneratorManager
            where TPrintGenerator : IPrintGenerator<TPrintSettings>, new()
            where TPrintSettings : SettingsPrototype, IPlanarAdditiveSettings, new()
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

        private static SettingsBuilderF DefaultSettingsBuilderF = (settings, logger) => new SettingsBuilder<TPrintSettings>(settings, logger);

        public PrintGeneratorManager(TPrintSettings settings, string id, string description, ILogger logger = null, bool acceptsParts = true, 
            SettingsBuilderF settingsBuilderF = null)
        {
            AcceptsParts = acceptsParts;

            Id = id;
            Description = description;

            this.logger = logger ?? new NullLogger();

            settingsBuilder = (settingsBuilderF ?? DefaultSettingsBuilderF).Invoke(settings, logger);
        }

        public GenerationResult GCodeFromMesh(DMesh3 mesh)
        {
            return GCodeFromMesh(mesh, null);
        }

        public GenerationResult GCodeFromMesh(DMesh3 mesh, TPrintSettings settings = null)
        {
            return GCodeFromMeshes(new DMesh3[] { mesh }, settings);
        }

        public GenerationResult GCodeFromMeshes(IEnumerable<DMesh3> meshes, TPrintSettings settings = null)
        {
            var printMeshAssembly = PrintMeshAssemblyFromMeshes(meshes);
            return GCodeFromPrintMeshAssembly(printMeshAssembly, settings);
        }

        public GenerationResult GCodeFromPrintMeshAssembly(PrintMeshAssembly printMeshAssembly, TPrintSettings settings = null)
        {
            PlanarSliceStack slices = null;

            if (AcceptsParts)
            {                
                var success = SliceMesh(printMeshAssembly, out slices);
                if (!success)
                {
                    var generationResult = new GenerationResult();
                    generationResult.AddLog(LoggingLevel.Error, "Mesh slicing failed");
                }
                    
            }

            var globalSettings = settings ?? settingsBuilder.Settings;

            // Run the print generator
            logger.WriteLine("Running print generator...");
            var printGenerator = new TPrintGenerator();
            AssemblerFactoryF overrideAssemblerF = globalSettings.AssemblerType();
            printGenerator.Initialize(printMeshAssembly, slices, globalSettings, overrideAssemblerF);

            return printGenerator.Generate();
        }

        private PrintMeshAssembly PrintMeshAssemblyFromMeshes(IEnumerable<DMesh3> meshes)
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
            LayerHeightMM = settings.LayerHeightMM
        };



        private bool SliceMesh(PrintMeshAssembly meshes, out PlanarSliceStack slices)
        {
            logger?.WriteLine("Slicing...");

            try
            {
                var slicer = GetSlicerF(Settings);

                slicer.Add(meshes);
                slices = slicer.Compute();
                return true;
            }
            catch (Exception e)
            {
                logger?.WriteLine(e.Message);
                if (Config.Debug)
                    throw;
                slices = null;
                return false;
            }
        }

        public GCodeFile LoadGCode(TextReader input)
        {
            GenericGCodeParser parser = new GenericGCodeParser();
            return parser.Parse(input);
        }

        public void SaveGCodeToFile(TextWriter output, GCodeFile file)
        {
            StandardGCodeWriter writer = new StandardGCodeWriter();
            writer.WriteFile(file, output);
        }
    }
}