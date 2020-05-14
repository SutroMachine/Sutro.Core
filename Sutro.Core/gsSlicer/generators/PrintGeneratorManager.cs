﻿using g3;
using gs.FillTypes;
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

        public PrintGeneratorManager(TPrintSettings settings, string id, string description, ILogger logger = null, bool acceptsParts = true)
        {
            AcceptsParts = acceptsParts;

            Id = id;
            Description = description;

            settingsBuilder = new SettingsBuilder<TPrintSettings>(settings, logger);
            this.logger = logger ?? new NullLogger();
        }

        public GCodeFile GCodeFromMesh(DMesh3 mesh, out IEnumerable<string> generationReport)
        {
            return GCodeFromMesh(mesh, out generationReport, null);
        }

        public GCodeFile GCodeFromMesh(DMesh3 mesh, out IEnumerable<string> generationReport, TPrintSettings settings = null)
        {
            return GCodeFromMeshes(new DMesh3[] { mesh }, out generationReport, settings);
        }

        public GCodeFile GCodeFromMeshes(IEnumerable<DMesh3> meshes, out IEnumerable<string> generationReport, TPrintSettings settings = null)
        {
            var printMeshAssembly = PrintMeshAssemblyFromMeshes(meshes);
            return GCodeFromPrintMeshAssembly(printMeshAssembly, out generationReport, settings);
        }

        public GCodeFile GCodeFromPrintMeshAssembly(PrintMeshAssembly printMeshAssembly, out IEnumerable<string> generationReport, TPrintSettings settings = null)
        {
            PlanarSliceStack slices = null;

            if (AcceptsParts)
            {
                SliceMesh(printMeshAssembly, out slices);
            }

            var globalSettings = settings ?? settingsBuilder.Settings;

            // Run the print generator
            logger.WriteLine("Running print generator...");
            var printGenerator = new TPrintGenerator();
            AssemblerFactoryF overrideAssemblerF = globalSettings.AssemblerType();
            printGenerator.Initialize(printMeshAssembly, slices, globalSettings, overrideAssemblerF);

            if (printGenerator.Generate())
            {
                generationReport = printGenerator.GenerationReport;
                return printGenerator.Result;
            }
            else
            {
                throw new Exception("PrintGenerator failed to generate gcode!");
            }
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

        private void SliceMesh(PrintMeshAssembly meshes, out PlanarSliceStack slices)
        {
            logger?.WriteLine("Slicing...");

            // Do slicing
            MeshPlanarSlicer slicer = new MeshPlanarSlicer()
            {
                LayerHeightMM = Settings.LayerHeightMM
            };

            slicer.Add(meshes);
            slices = slicer.Compute();
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