using System;

namespace gs
{
    public class ElementLocation
    {
        public ElementLocation(int index, double distance)
        {
            Index = index;
            ParameterizedDistance = distance;
        }

        public int Index { get; set; }

        private double parameterizedDistance = 0;

        public double ParameterizedDistance
        {
            get => parameterizedDistance;
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentException("Parameterized distance must be between 0 and 1 (inclusive)");
                }
                parameterizedDistance = value;
            }
        }
    }
}