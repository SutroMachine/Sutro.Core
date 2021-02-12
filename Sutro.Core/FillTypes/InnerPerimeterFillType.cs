namespace gs.FillTypes
{
    public class InnerPerimeterFillType : BaseFillType
    {
        public static string Label => "inner perimeter";

        public override string GetLabel()
        {
            return Label;
        }

        public InnerPerimeterFillType(double volumeScale = 1, double speedScale = 1) : base(volumeScale, speedScale)
        {
        }

        public override bool IsPartShell()
        {
            return true;
        }
    }
}