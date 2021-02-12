namespace gs.FillTypes
{
    public class SolidFillType : BaseFillType
    {
        public SolidFillType(double volumeScale = 1, double speedScale = 1) : base(volumeScale, speedScale)
        {
        }

        public static string Label => "solid layer";

        public override string GetLabel()
        {
            return Label;
        }
    }
}