using g3;
using Sutro.Core.FillTypes;

namespace Sutro.Core.Fill
{
    public abstract class FillBase
    {
        // General Properties
        public IFillType FillType { get; set; } = new DefaultFillType();

        public double FillThickness { get; set; }
        public bool IsHoleShell { get; set; } = false;
        public int PerimeterOrder { get; set; } = -1;

        public virtual void CopyProperties(FillBase other)
        {
            FillType = other.FillType;
            FillThickness = other.FillThickness;
            IsHoleShell = other.IsHoleShell;
            PerimeterOrder = other.PerimeterOrder;
        }

        public abstract Vector2d Entry { get; }
        public abstract Vector2d Exit { get; }
        public abstract int ElementCount { get; }

        public abstract Vector3d GetVertex(int index);

        public abstract Segment2d GetSegment2d(int index);

        public abstract double TotalLength();

        public abstract double FindClosestElementToPoint(Vector2d point, out ElementLocation location);
    }
}