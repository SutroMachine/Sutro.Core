using g3;
using Sutro.Core.Toolpaths;
using System.Collections.Generic;

namespace Sutro.Core.Utility
{
    /// <summary>
    /// This class calculates the extrusion and travel times for a path set.
    /// Also has ScaleExtrudeTimes(), which scales the speed of each of the
    /// extrusions, and can be used to hit a specific layer time.
    ///
    /// (note that acceleration is not considered, so the times are a best-case scenario)
    ///
    /// EnforceMinLayerTime() is a utility that automatically re-times layers
    /// so they take at least a specified minimum time.
    /// </summary>
    public class CalculatePrintTime
    {
        public ToolpathSet Paths { get; }

        // output statistics
        public PrintTimeStatistics TimeStatistics { get; private set; }

        public CalculatePrintTime(ToolpathSet paths)
        {
            Paths = paths;
        }

        /// <summary>
        /// Calculate the extrusion and travel times
        /// </summary>
        public void Calculate()
        {
            // [TODO] could do this inline...

            // filter paths
            List<IToolpath> allPaths = new List<IToolpath>();
            foreach (IToolpath ipath in Paths)
            {
                ToolpathUtil.ApplyToLeafPaths(ipath, (p) =>
                {
                    if (p is LinearToolpath3<PrintVertex>)
                    {
                        allPaths.Add(p);
                    }
                });
            }
            int N = allPaths.Count;

            TimeStatistics = new PrintTimeStatistics();

            for (int pi = 0; pi < N; ++pi)
            {
                LinearToolpath3<PrintVertex> path = allPaths[pi] as LinearToolpath3<PrintVertex>;
                if (path == null || path.Type != ToolpathTypes.Deposition && path.Type != ToolpathTypes.Travel)
                    continue;

                double path_time = 0;
                Vector3d curPos = path[0].Position;
                for (int i = 1; i < path.VertexCount; ++i)
                {
                    bool last_vtx = i == path.VertexCount - 1;

                    Vector3d newPos = path[i].Position;
                    double newRate = path[i].FeedRate;

                    double dist = (newPos - curPos).Length;

                    double rate_mm_per_s = newRate / 60;    // feed rates are in mm/min
                    path_time += dist / rate_mm_per_s;
                    curPos = newPos;
                }
                if (path.Type == ToolpathTypes.Deposition)
                    TimeStatistics.ExtrudeTimeS += path_time;
                else
                    TimeStatistics.TravelTimeS += path_time;
            }
        } // Calculate()

        /// <summary>
        /// Scale the extrusion speeds by the given scale factor
        /// </summary>
        public void ScaleExtrudeTimes(double scaleFactor, double minimumExtrusionSpeed)
        {
            // filter paths
            foreach (IToolpath ipath in Paths)
            {
                ToolpathUtil.ApplyToLeafPaths(ipath, (p) =>
                {
                    if (p is LinearToolpath3<PrintVertex>)
                    {
                        LinearToolpath3<PrintVertex> path = p as LinearToolpath3<PrintVertex>;
                        if (path != null && path.Type == ToolpathTypes.Deposition)
                        {
                            for (int i = 0; i < path.VertexCount; ++i)
                            {
                                PrintVertex v = path[i];
                                double rate = path[i].FeedRate;
                                double scaledRate = v.FeedRate * scaleFactor;
                                if (scaledRate < minimumExtrusionSpeed)
                                    scaledRate = minimumExtrusionSpeed;
                                v.FeedRate = scaledRate;
                                path.UpdateVertex(i, v);
                            }
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Enforce a minimum layer time on this path set
        /// Returns true if modification was required
        /// </summary>
        public bool EnforceMinLayerTime(double minimumLayerTime, double minimumExtrusionSpeed)
        {
            Calculate();
            if (TimeStatistics.TotalTimeS < minimumLayerTime)
            {
                double targetExtrusionTime = minimumLayerTime - TimeStatistics.TravelTimeS;
                double depositionSpeedScaleFactor = TimeStatistics.ExtrudeTimeS / targetExtrusionTime;
                ScaleExtrudeTimes(depositionSpeedScaleFactor, minimumExtrusionSpeed);
                return true;
            }
            return false;
        }
    }
}