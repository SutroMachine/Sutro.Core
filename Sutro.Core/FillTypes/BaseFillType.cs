namespace gs.FillTypes
{
    public abstract class BaseFillType : IFillType
    {
        protected readonly double volumeScale = 1;
        protected readonly double speedScale = 1;

        public BaseFillType(double volumeScale = 1, double speedScale = 1)
        {
            this.volumeScale = volumeScale;
            this.speedScale = speedScale;
        }

        public virtual double AdjustVolume(double volume)
        {
            return volume * volumeScale;
        }

        public abstract string GetLabel();

        public virtual double ModifySpeed(double speed, SpeedHint speedHint)
        {
            return speed * speedScale;
        }

        public virtual bool IsEntryLocationSpecified()
        {
            return false;
        }

        public virtual bool IsPartShell()
        {
            return false;
        }
    }
}