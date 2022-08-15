using System.Collections.Generic;

namespace Sutro.Core.FunctionalTest
{
    public interface IFeatureInfo
    {
        string FillType { get; set; }

        void Add(IFeatureInfo other);

        IEnumerable<Comparison> Compare(IFeatureInfo other);
    }
}