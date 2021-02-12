namespace Sutro.Core.Test
{
    public interface IFeatureInfo
    {
        string FillType { get; set; }

        void Add(IFeatureInfo other);

        void AssertEqualsExpected(IFeatureInfo other);
    }
}