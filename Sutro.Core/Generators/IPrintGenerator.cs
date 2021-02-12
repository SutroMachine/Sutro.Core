using Sutro.Core.Assemblers;
using Sutro.Core.Models.GCode;
using Sutro.Core.Slicing;
using System.Collections.Generic;
using System.Threading;

namespace Sutro.Core.Generators
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