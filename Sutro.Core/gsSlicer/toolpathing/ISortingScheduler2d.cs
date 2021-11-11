using g3;

namespace gs
{
    public interface ISortingScheduler2d : IFillPathScheduler2d
    {
        void SortAndAppendTo(Vector2d lastPoint, IFillPathScheduler2d targetScheduler);
    }
}