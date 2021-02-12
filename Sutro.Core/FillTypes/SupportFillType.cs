namespace gs.FillTypes
{
    public class SupportFillType : BaseFillType
    {
        private readonly double carefulSpeedScale;

        public static string Label => "support";

        public override string GetLabel()
        {
            return Label;
        }

        public SupportFillType(double volumeScale = 1, double carefulSpeedScale = 1) : base(volumeScale)
        {
            this.carefulSpeedScale = carefulSpeedScale;
        }

        public override double ModifySpeed(double speed, SpeedHint speedHint = SpeedHint.Default)
        {
            if (speedHint == SpeedHint.Careful)
                return speed * carefulSpeedScale;
            else
                return speed;
        }

        public bool IsPart()
        {
            return false;
        }
    }
}