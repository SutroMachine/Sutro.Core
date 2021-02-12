using System;

namespace Sutro.Core.Fill
{
    public interface IFillSegment
    {
        bool IsConnector { get; }

        IFillSegment Reversed();

        Tuple<IFillSegment, IFillSegment> Split(double t);
    }
}