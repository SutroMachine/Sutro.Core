using System;
using System.Collections.Generic;
using System.Text;

namespace Sutro.Core.FunctionalTest
{
    public class ComparisonReport
    {
        private readonly List<string> summaryMismatches = new List<string>();
        private readonly List<string> totalMismatches = new List<string>();
        public bool AreEquivalent { get; private set; } = true;

        public void AddSummaryMismatch(string message)
        {
            AreEquivalent = false;
            summaryMismatches.Add(message);
        }

        public void AddTotalMismatch(string message)
        {
            AreEquivalent = false;
            totalMismatches.Add(message);
        }

        public string GetReport()
        {
            if (AreEquivalent)
            {
                return "✅ Print files are equivalent";
            }
            var sb = new StringBuilder();

            sb.AppendLine("❌ Print files are not the same!");
            sb.AppendLine("SUMMARY:");
            foreach (var mismatch in summaryMismatches)
            {
                sb.Append("    ").AppendLine(mismatch);
            }
            sb.AppendLine("TOTAL:");
            foreach (var mismatch in totalMismatches)
            {
                sb.Append("    ").AppendLine(mismatch);
            }

            return sb.ToString();
        }
    }
}