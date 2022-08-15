using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Sutro.Core.FunctionalTest
{
    public class LayerInfo<TFeatureInfo> where TFeatureInfo : IFeatureInfo, new()
    {
        public LayerInfo()
        {
        }

        private readonly Dictionary<string, TFeatureInfo> perFeatureInfo =
            new Dictionary<string, TFeatureInfo>();

        public Dictionary<string, ReadOnlyCollection<Comparison>> Compare(LayerInfo<TFeatureInfo> expected)
        {
            var combinedKeys = new HashSet<string>();
            foreach (var key in perFeatureInfo.Keys)
            {
                combinedKeys.Add(key);
                if (!expected.perFeatureInfo.ContainsKey(key))
                    expected.perFeatureInfo[key] = new TFeatureInfo();
            }

            foreach (var key in expected.perFeatureInfo.Keys)
            {
                combinedKeys.Add(key);
                if (!perFeatureInfo.ContainsKey(key))
                    perFeatureInfo[key] = new TFeatureInfo();
            }

            var comparisons = new Dictionary<string, ReadOnlyCollection<Comparison>>();
            foreach (var fillType in combinedKeys)
            {
                comparisons[fillType] = perFeatureInfo[fillType].Compare(expected.perFeatureInfo[fillType]).ToList().AsReadOnly();
            }
            return comparisons;
        }

        public bool GetFeatureInfo(string fillType, out TFeatureInfo featureInfo)
        {
            return perFeatureInfo.TryGetValue(fillType, out featureInfo);
        }

        public IEnumerable<string> GetFillTypes()
        {
            foreach (var key in perFeatureInfo.Keys)
            {
                yield return key;
            }
        }

        public void AddFeatureInfo(TFeatureInfo featureInfo)
        {
            if (featureInfo == null)
                return;

            if (GetFeatureInfo(featureInfo.FillType, out var existingFeatureInfo))
            {
                existingFeatureInfo.Add(featureInfo);
            }
            else
            {
                perFeatureInfo.Add(featureInfo.FillType, featureInfo);
            }
        }
    }
}