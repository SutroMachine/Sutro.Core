using g3;

namespace gs
{
    public interface ISortingScheduler2d : IFillPathScheduler2d
    {
        void SortAndAppendTo(Vector2d startPoint, IFillPathScheduler2d targetScheduler);
    }
}