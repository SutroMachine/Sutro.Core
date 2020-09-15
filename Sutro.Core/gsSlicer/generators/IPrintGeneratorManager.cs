using g3;
using Sutro.Core.Models.GCode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace gs
{
    public interface IPrintGeneratorManager
    {
        string Id { get; }
        string Description { get; }

        Version PrintGeneratorAssemblyVersion { get; }
        string PrintGeneratorAssemblyName { get; }
        string PrintGeneratorName { get; }

        bool AcceptsParts { get; }
        ISettingsBuilder SettingsBuilder { get; }

        GCodeFile GCodeFromMesh(DMesh3 mesh, out IEnumerable<string> generationReport, CancellationToken? cancellationToken = null);

        void SaveGCodeToFile(TextWriter output, GCodeFile file);
    }
}