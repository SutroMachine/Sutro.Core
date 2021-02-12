using Sutro.Core;
using Sutro.Core.Models.GCode;
using System.Collections.Generic;
using System.Threading;

namespace gs
{
    public interface IPrintGenerator<TPrintSettings>
    {
        void Initialize(
            PrintMeshAssembly meshes,
            PlanarSliceStack slices,
            TPrintSettings settings,
            AssemblerFactoryF overrideAssemblerF);

        GenerationResult Generate(CancellationToken? cancellationToken);

        GCodeFile Result { get; }

        IReadOnlyList<string> PrintTimeEstimate { get; }
        IReadOnlyList<string> MaterialUsageEstimate { get; }
        IReadOnlyList<string> Warnings { get; }
    }
}