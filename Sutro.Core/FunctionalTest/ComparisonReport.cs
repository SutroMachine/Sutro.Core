using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Sutro.Core.FunctionalTest
{
    public class ComparisonReport
    {
        private readonly StringBuilder sectionSummary = new StringBuilder();
        private readonly StringBuilder sectionTotal = new StringBuilder();
        private readonly StringBuilder sectionLayers = new StringBuilder();

        public ComparisonReport()
        {
            sectionSummary.AppendLine("SUMMARY:");
            sectionTotal.AppendLine("ALL LAYERS:");
            sectionLayers.AppendLine("PER LAYER:");
        }
        public bool AreEquivalent { get; private set; } = true;

        public void AddSummary(Comparison comparison)
        {
            AreEquivalent &= comparison.Match;
            sectionSummary.AppendLine($"    {comparison.Message}");
        }

        public string GetReport()
        {
            if (AreEquivalent)
            {                
                return "Print files are equivalent";
            }
            var sb = new StringBuilder();

            sb.AppendLine("Print files are not the same!");
            sb.Append(sectionSummary);
            sb.Append(sectionTotal);
            sb.Append(sectionLayers);

            return sb.ToString();
        }

        public void AddTotal(string featureType, ReadOnlyCollection<Comparison> comparison)
        {
            // Don't add the feature if everything in it matches
            if (comparison.All(comp => comp.Match))
                return;

            AreEquivalent = false;

            sectionTotal.AppendLine($"    {featureType}:");
            foreach (var feature in comparison.Where(comp => !comp.Match))
            {
                sectionTotal.AppendLine($"        {feature.Message}");
            }
        }

        public void AddLayer(int layerIndex, Dictionary<string, ReadOnlyCollection<Comparison>> features)
        {
            // Don't add the layer if everything in it matches
            if (features.All(f => f.Value.All(c => c.Match)))
                return;

            AreEquivalent = false;

            sectionLayers.AppendLine($"    Layer #{layerIndex}:");
            foreach (var feature in features)
            {
                // Don't add the feature if everything in it matches
                if (feature.Value.All(comp => comp.Match))
                    continue;

                sectionLayers.AppendLine($"        {feature.Key}:");
                foreach (var c in feature.Value.Where(c => !c.Match))
                {
                    sectionLayers.AppendLine($"            {c.Message}");
                }
            }
        }
    }
}