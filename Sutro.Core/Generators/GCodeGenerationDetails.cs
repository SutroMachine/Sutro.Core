using System.Collections.Generic;
using System.Linq;

namespace Sutro.Core.Generators
{
    public struct GCodeGenerationDetails
    {
        public GCodeGenerationDetails(
            IEnumerable<string> printTimeEstimate,
            IEnumerable<string> materialUsageEstimate,
            IEnumerable<string> warnings = null)
        {
            PrintTimeEstimate = printTimeEstimate.ToList();
            MaterialUsageEstimate = materialUsageEstimate.ToList();
            Warnings = warnings?.ToList() ?? new List<string>();
        }

        public IReadOnlyCollection<string> PrintTimeEstimate { get; }
        public IReadOnlyCollection<string> MaterialUsageEstimate { get; }
        public IReadOnlyCollection<string> Warnings { get; }
    }
}