using System;
using System.Collections.Generic;

namespace gs
{
    public class PrintTimeStatistics
    {
        public double ExtrudeTimeS { get; set; } = 0;
        public double TravelTimeS { get; set; } = 0;

        public double TotalTimeS
        {
            get
            {
                return ExtrudeTimeS + TravelTimeS;
            }
        }

        public void Add(PrintTimeStatistics other)
        {
            ExtrudeTimeS += other.ExtrudeTimeS;
            TravelTimeS += other.TravelTimeS;
        }

        public List<string> ToStringList()
        {
            return new List<string>()
            {
                "TOTAL PRINT TIME ESTIMATE:",
                $"        Total: {new TimeSpan(0, 0, (int)TotalTimeS):c}",
                $"    Extrusion: {new TimeSpan(0, 0, (int)ExtrudeTimeS):c}    ({ExtrudeTimeS/TotalTimeS*100,4:##.0}%)",
                $"       Travel: {new TimeSpan(0, 0, (int)TravelTimeS):c}    ({TravelTimeS/TotalTimeS*100,4:##.0}%)",
            };
        }
    }
}