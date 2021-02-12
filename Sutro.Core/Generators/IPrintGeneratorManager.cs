using g3;
using Sutro.Core.Models.GCode;
using Sutro.Core.Utility;
using System;
using System.IO;
using System.Threading;

namespace Sutro.Core.Generators
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