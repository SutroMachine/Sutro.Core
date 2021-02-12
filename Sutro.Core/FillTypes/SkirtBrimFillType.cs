namespace gs.FillTypes
{
    public class SkirtBrimFillType : BaseFillType
    {
        public static string Label => "skirt";

        public override string GetLabel()
        {
            return Label;
        }

        public override bool IsEntryLocationSpecified()
        {
            return true;
        }
    }
}