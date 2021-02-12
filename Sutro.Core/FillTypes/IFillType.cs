namespace gs.FillTypes
{
    public interface IFillType
    {
        double ModifySpeed(double speed, SpeedHint speedHint);

        double AdjustVolume(double volume);

        string GetLabel();

        bool IsEntryLocationSpecified();

        bool IsPart() { return true; }

        bool IsPartShell();
    }
}