using g3;
using Sutro.Core;
using Sutro.Core.Models.GCode;
using System;
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

        GenerationResult GCodeFromMesh(DMesh3 mesh, CancellationToken? cancellationToken);

        void SaveGCodeToFile(TextWriter output, GCodeFile file);
    }
}