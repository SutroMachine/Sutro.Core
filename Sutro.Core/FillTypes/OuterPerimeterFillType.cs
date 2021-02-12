namespace gs.FillTypes
{
    public class OuterPerimeterFillType : BaseFillType
    {
        public static string Label => "outer perimeter";

        public override string GetLabel()
        {
            return Label;
        }

        public OuterPerimeterFillType(double volumeScale = 1, double speedScale = 1) : base(volumeScale, speedScale)
        {
        }

        public override bool IsEntryLocationSpecified()
        {
            return true;
        }

        public override bool IsPartShell()
        {
            return true;
        }
    }
}