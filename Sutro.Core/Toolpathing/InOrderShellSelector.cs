using g3;
using System.Collections.Generic;

namespace Sutro.Core.Toolpathing
{
    public class InOrderShellSelector : ILayerShellsSelector
    {
        public List<IShellsFillPolygon> LayerShells;
        private int iCurrent;

        public InOrderShellSelector(List<IShellsFillPolygon> shells)
        {
            LayerShells = shells;
            iCurrent = 0;
        }

        public IShellsFillPolygon Next(Vector2d currentPosition)
        {
            if (iCurrent < LayerShells.Count)
                return LayerShells[iCurrent++];
            else
                return null;
        }
    }
}