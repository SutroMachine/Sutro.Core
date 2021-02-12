using System;

namespace gs
{
    public readonly struct FillSegment : IFillSegment
    {
        public FillSegment(bool isConnector = false)
        {
            IsConnector = isConnector;
        }

        public bool IsConnector { get; }

        public FillSegment Reversed()
        {
            return new FillSegment(this);
        }

        IFillSegment IFillSegment.Reversed()
        {
            return Reversed();
        }

        public Tuple<IFillSegment, IFillSegment> Split(double t)
        {
            return Tuple.Create((IFillSegment)new FillSegment(this), (IFillSegment)new FillSegment(this));
        }

        public FillSegment(FillSegment other)
        {
            IsConnector = other.IsConnector;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is FillSegment other))
                return false;
            return other.IsConnector == IsConnector;
        }

        public override int GetHashCode()
        {
            return Tuple.Create(IsConnector).GetHashCode();
        }
    }
}