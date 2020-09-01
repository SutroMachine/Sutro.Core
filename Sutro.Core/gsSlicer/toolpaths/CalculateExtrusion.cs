using g3;
using Sutro.Core.Settings;
using System;
using System.Collections.Generic;

namespace gs
{
    /// <summary>
    /// This class implements calculation of the filament extrusion distance/volume along
    /// a ToolpathSet. Currently the actual extrusion calculation is quite basic.
    ///
    /// Note that this is (currently) also where retraction happens. Possibly this would
    /// be better handled elsewhere, since ideally it is a temporary +/- to the extrusion,
    /// such that the actual accumulated extrusion amount is not modified.
    ///
    /// </summary>
	public class CalculateExtrusion<T> where T : IExtrusionVertex
    {
        public IEnumerable<LinearToolpath3<T>> Paths;
        public PrintProfileFFF Settings;

        public bool EnableRetraction = true;

        private double FilamentDiam = 1.75;
        private double NozzleDiam = 0.4;
        private double LayerHeight = 0.2;
        private double FixedRetractDistance = 1.3;

        // [RMS] if we travel less than this distance, we don't retract
        private double MinRetractTravelLength = 2.5;

        // output statistics
        public int NumPaths = 0;

        public double ExtrusionLength = 0;

        public CalculateExtrusion(IEnumerable<LinearToolpath3<T>> paths, PrintProfileFFF settings)
        {
            Paths = paths;
            Settings = settings;

            EnableRetraction = settings.Part.EnableRetraction;
            FilamentDiam = settings.Material.FilamentDiamMM;
            NozzleDiam = settings.Machine.NozzleDiamMM;
            LayerHeight = settings.LayerHeightMM;
            FixedRetractDistance = settings.Part.RetractDistanceMM;
            MinRetractTravelLength = settings.Part.MinRetractTravelLength;
        }

        public void Calculate(Vector3d vStartPos, double fStartA, bool alreadyInRetract = false)
        {
            double curA = fStartA;
            Vector3d curPos = vStartPos;
            double curRate = 0;

            bool inRetract = alreadyInRetract;

            // filter paths
            List<IToolpath> allPaths = new List<IToolpath>();
            foreach (IToolpath p in Paths)
            {
                if (p is LinearToolpath3<T> || p is ResetExtruderPathHack)
                {
                    allPaths.Add(p);
                }
            }

            int N = allPaths.Count;

            LinearToolpath3<T> prev_path = null;
            for (int pi = 0; pi < N; ++pi)
            {
                if (allPaths[pi] is ResetExtruderPathHack)
                {
                    curA = 0;
                    continue;
                }
                var path = allPaths[pi] as LinearToolpath3<T>;

                if (path == null)
                    throw new Exception("Invalid path type!");
                if (!(path.Type == ToolpathTypes.Deposition || path.Type == ToolpathTypes.PlaneChange || path.Type == ToolpathTypes.Travel))
                    throw new Exception("Unknown path type!");

                // if we are travelling between two extrusion paths, and neither is support,
                // and the travel distance is very short,then we will skip the retract.
                // [TODO] should only do this on interior travels. We should determine that upstream and set a flag on travel path.
                bool skip_retract = false;
                if (path.Type == ToolpathTypes.Travel && path.Length < MinRetractTravelLength)
                {
                    bool prev_is_model_deposition =
                        (prev_path != null) && (prev_path.Type == ToolpathTypes.Deposition) && prev_path.FillType.IsPart();
                    var next_path = (pi < N - 1) ? (allPaths[pi + 1] as LinearToolpath3<T>) : null;
                    bool next_is_model_deposition =
                        (next_path != null) && (next_path.Type == ToolpathTypes.Deposition) && next_path.FillType.IsPart();
                    skip_retract = prev_is_model_deposition && next_is_model_deposition;
                }
                if (EnableRetraction == false)
                    skip_retract = true;

                // figure out volume scaling based on path type
                double vol_scale = 1.0;
                vol_scale = path.FillType.AdjustVolume(vol_scale);

                for (int i = 0; i < path.VertexCount; ++i)
                {
                    bool last_vtx = (i == path.VertexCount - 1);

                    Vector3d newPos = path[i].Position;
                    double newRate = path[i].FeedRate;

                    // default line thickness and height
                    double path_width = Settings.Machine.NozzleDiamMM;
                    double path_height = Settings.LayerHeightMM;

                    // override with custom dimensions if provided
                    Vector2d vtx_dims = path[i].Dimensions;
                    if (vtx_dims.x > 0 && vtx_dims.x < 1000.0)
                        path_width = vtx_dims.x;
                    if (vtx_dims.y > 0 && vtx_dims.y < 1000.0)
                        path_height = vtx_dims.y;

                    if (path.Type != ToolpathTypes.Deposition)
                    {
                        // [RMS] if we switched to a travel move we retract, unless we don't
                        if (skip_retract == false)
                        {
                            if (!inRetract)
                            {
                                curA -= FixedRetractDistance;
                                inRetract = true;
                            }
                        }

                        curPos = newPos;
                        curRate = newRate;
                    }
                    else
                    {
                        // for i == 0 this dist is always 0 !!
                        double dist = (newPos - curPos).Length;

                        if (i == 0)
                        {
                            Util.gDevAssert(dist < 1e-12);     // next path starts at end of previous!!
                            if (inRetract)
                            {
                                curA += FixedRetractDistance;
                                inRetract = false;
                            }
                        }
                        else
                        {
                            curPos = newPos;
                            curRate = newRate;

                            double segment_width = (path[i].Dimensions.x != GCodeUtil.UnspecifiedValue) ?
                                path[i].Dimensions.x : path_width;

                            double segment_height = (path[i].Dimensions.y != GCodeUtil.UnspecifiedValue) ?
                                path[i].Dimensions.y : path_height;

                            double feed = ExtrusionMath.PathLengthToFilamentLength(
                                segment_height, segment_width, Settings.Material.FilamentDiamMM,
                                dist, vol_scale);

                            // Change the extrusion amount if a modifier is present.
                            // TODO: This is a bit of a hack since the modifier acts on a Vector3d
                            //       and we're ignoring data in the second & third positions.
                            var modifier = path[i]?.ExtendedData?.ExtrusionModifierF;
                            if (modifier != null)
                                feed = modifier(new Vector3d(feed, 0, 0))[0];

                            curA += feed;
                        }
                    }

                    T v = path[i];
                    v.Extrusion = GCodeUtil.Extrude(curA);
                    path.UpdateVertex(i, v);
                }

                prev_path = path;
            }

            NumPaths = N;
            ExtrusionLength = curA;
        } // Calculate()
    }
}