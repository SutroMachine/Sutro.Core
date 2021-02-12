using g3;
using System.Collections.Generic;

namespace gs
{
    public class NextNearestLayerShellsSelector : ILayerShellsSelector
    {
        public List<IShellsFillPolygon> LayerShells;
        private HashSet<IShellsFillPolygon> remaining;

        public NextNearestLayerShellsSelector(List<IShellsFillPolygon> shells)
        {
            LayerShells = shells;
            remaining = new HashSet<IShellsFillPolygon>(shells);
        }

        public IShellsFillPolygon Next(Vector2d currentPosition)
        {
            if (remaining.Count == 0)
                return null;

            IShellsFillPolygon nearest = null;
            double nearest_dist = double.MaxValue;
            foreach (IShellsFillPolygon shell in remaining)
            {
                double dist = shell.Polygon.Outer.DistanceSquared(currentPosition);
                if (dist < nearest_dist)
                {
                    nearest_dist = dist;
                    nearest = shell;
                }
            }
            remaining.Remove(nearest);
            return nearest;
        }
    }
}