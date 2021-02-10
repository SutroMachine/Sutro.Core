using System;

namespace gs
{
    public interface IFillSegment
    {
        bool IsConnector { get; }

        IFillSegment Reversed();

        Tuple<IFillSegment, IFillSegment> Split(double t);
    }
}