using gs;
using Sutro.Core.FunctionalTest.FeatureMismatchExceptions;
using System.Collections.Generic;

namespace Sutro.Core.FunctionalTest
{
    public class LayerInfo<TFeatureInfo> where TFeatureInfo : IFeatureInfo
    {
        public LayerInfo()
        {
        }

        private readonly Dictionary<string, TFeatureInfo> perFeatureInfo =
            new Dictionary<string, TFeatureInfo>();

        public IEnumerable<string> Compare(LayerInfo<TFeatureInfo> expected)
        {
            foreach (var key in perFeatureInfo.Keys)
                if (!expected.perFeatureInfo.ContainsKey(key))
                    yield return $"Result has unexpected feature {key}";

            foreach (var key in expected.perFeatureInfo.Keys)
                if (!perFeatureInfo.ContainsKey(key))
                    yield return ($"Result was missing expected feature {key}");

            foreach (var fillType in perFeatureInfo.Keys)
            {
                foreach (var mismatch in perFeatureInfo[fillType].Compare(expected.perFeatureInfo[fillType]))
                {
                    yield return mismatch;
                }
            }
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