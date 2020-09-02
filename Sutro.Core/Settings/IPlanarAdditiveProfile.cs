namespace Sutro.Core.Settings
{
    public interface IPlanarAdditiveProfile
    {
        double LayerHeightMM { get; set; }
        IPlanarAdditiveProfile Clone();
    }
}