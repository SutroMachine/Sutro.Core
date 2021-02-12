using g3;
using System;
using System.Collections.Generic;

namespace gs
{
    /// <summary>
    /// Attempt to remove self-overlaps in a set of 2D polygons
    ///
    /// [TODO]
    ///   - MinSelfSegDistance would greatly benefit from some kind of spatial data structure
    ///   - Resampling is currently unconstrained...
    ///   - expose parameters/etc
    ///
    /// </summary>
    public class PathOverlapRepair
    {
        public DGraph2 Graph;

        public double OverlapRadius = 0.4f;
        public double CollisionRadius = 0.4f;
        public double FinalFlatCollapseAngleThreshDeg = 0.1;

        public Func<int, bool> PreserveEdgeFilterF = (eid) => { return false; };

        private DGraph2 CollisionGraph = new DGraph2();

        public PathOverlapRepair()
        {
            Graph = new DGraph2();
        }

        public PathOverlapRepair(DGraph2 graph)
        {
            Graph = graph;
        }

        public void Add(GeneralPolygon2d path, int outer_gid = -1, int hole_gid = -1)
        {
            Graph.AppendPolygon(path.Outer, outer_gid);
            foreach (Polygon2d hole in path.Holes)
                Graph.AppendPolygon(hole, hole_gid);
        }

        public void Add(PolyLine2d path, int gid = -1)
        {
            Graph.AppendPolyline(path, gid);
        }

        /// <summary>
        /// Collision paths are fixed thickened paths we need to clip against
        /// </summary>
        public void AddCollisionConstraint(PolyLine2d path)
        {
            CollisionGraph.AppendPolyline(path);
        }

        /// <summary>
        /// Collision polygons are fixed thickened paths we need to clip against
        /// </summary>
        public void AddCollisionConstraint(Polygon2d poly)
        {
            CollisionGraph.AppendPolygon(poly);
        }

        public void Compute()
        {
            FilterSelfOverlaps(OverlapRadius);
        }

        public DGraph2 GetResultGraph()
        {
            return Graph;
        }

        private bool is_fixed_v(int vid)
        {
            foreach (int eid in Graph.VtxEdgesItr(vid))
            {
                if (PreserveEdgeFilterF(eid))
                    return true;
            }
            return false;
        }

        private SegmentHashGrid2d<int> edge_hash;    // edge hash table used inside FilterSelfOverlaps
        private SegmentHashGrid2d<int> collision_edge_hash;

        private void FilterSelfOverlaps(double overlapRadius, bool bResample = true)
        {
            // [RMS] this tolerance business is not workign properly right now. The problem is
            //  that decimator loses corners!

            // To simplify the computation we are going to resample the curve so that no adjacent
            // are within a given distance. Then we can use distance-to-segments, with the two adjacent
            // segments filtered out, to measure self-distance

            double distanceThreshold = overlapRadius;
            double sharpCornerThresholdAngleDeg = 45;

            // resample graph. the degenerate-edge thing is necessary to
            // filter out tiny segments that are functionally sharp corners,
            // but geometrically are made of multiple angles < threshold
            // (maybe there is a better way to do this?)
            var resampler = new DGraph2Resampler(Graph);
            ResampleGraph(resampler, overlapRadius, bResample);

            // Find sharp corners
            var sharpCorners = FindSharpCorners(sharpCornerThresholdAngleDeg);

            // Disconnect at sharp corners
            DisconnectAtSharpCorners(sharpCorners);

            // Build edge hash table  (cell size is just a ballpark guess here...)
            BuildEdgeHashTable(overlapRadius);

            // Step 1: erode from boundary vertices
            ErodeFromBoundaryVertices(distanceThreshold);

            // Step 2: find any other possible self-overlaps and erode them.
            var remainingVertices = ErodeOtherSelfOverlaps();

            // Look for overlap vertices. When we find one, erode on both sides.
            ErodeFromOverlapVertices(distanceThreshold, remainingVertices);

            // Get rid of extra vertices
            resampler.CollapseFlatVertices(FinalFlatCollapseAngleThreshDeg);
        }

        private void BuildEdgeHashTable(double overlapRadius)
        {
            edge_hash = new SegmentHashGrid2d<int>(3 * overlapRadius, -1);
            foreach (int eid in Graph.EdgeIndices())
            {
                Segment2d seg = Graph.GetEdgeSegment(eid);
                edge_hash.InsertSegment(eid, seg.Center, seg.Extent);
            }

            if (CollisionGraph.EdgeCount > 0)
            {
                collision_edge_hash = new SegmentHashGrid2d<int>(3 * CollisionRadius, -1);
                foreach (int eid in CollisionGraph.EdgeIndices())
                {
                    Segment2d seg = CollisionGraph.GetEdgeSegment(eid);
                    collision_edge_hash.InsertSegment(eid, seg.Center, seg.Extent);
                }
            }
        }

        private void ErodeFromOverlapVertices(double distanceThreshold, List<Vector2d> remainingVertices)
        {
            foreach (Vector2d vinfo in remainingVertices)
            {
                int vid = (int)vinfo.x;
                if (Graph.IsVertex(vid) == false)
                    continue;
                double dist = MinSelfSegDistance(vid, 2 * distanceThreshold);
                if (dist < distanceThreshold)
                {
                    List<int> nbrs = new List<int>(Graph.GetVtxEdges(vid));
                    foreach (int eid in nbrs)
                    {
                        if (Graph.IsEdge(eid))    // may have been decimated!
                            decimate_forward(vid, eid, distanceThreshold);
                    }
                }
            }
        }

        private List<Vector2d> ErodeOtherSelfOverlaps()
        {
            // sort all vertices by opening angle. For any overlap, we can erode
            // on either side. Prefer to erode on side with higher curvature.
            List<Vector2d> remaining_v = new List<Vector2d>(Graph.MaxVertexID);
            foreach (int vid in Graph.VertexIndices())
            {
                if (is_fixed_v(vid))
                    continue;
                double open_angle = Graph.OpeningAngle(vid);
                if (open_angle == double.MaxValue)
                    continue;
                remaining_v.Add(new Vector2d(vid, open_angle));
            }
            remaining_v.Sort((a, b) => { return (a.y < b.y) ? -1 : (a.y > b.y ? 1 : 0); });
            return remaining_v;
        }

        private void ErodeFromBoundaryVertices(double distanceThreshold)
        {
            List<int> boundaries = new List<int>();
            foreach (int vid in Graph.VertexIndices())
            {
                if (Graph.GetVtxEdgeCount(vid) == 1)
                    boundaries.Add(vid);
            }
            foreach (int vid in boundaries)
            {
                if (Graph.IsVertex(vid) == false)
                    continue;
                double dist = MinSelfSegDistance(vid, 2 * distanceThreshold);
                double collision_dist = MinCollisionConstraintDistance(vid, CollisionRadius);
                if (dist < distanceThreshold || collision_dist < CollisionRadius)
                {
                    int eid = Graph.GetVtxEdges(vid)[0];
                    decimate_forward(vid, eid, distanceThreshold);
                }
            }
        }

        private void DisconnectAtSharpCorners(List<int> sharp_corners)
        {
            foreach (int vid in sharp_corners)
            {
                if (Graph.IsVertex(vid) == false)
                    continue;
                int e0 = Graph.GetVtxEdges(vid)[0];
                Index2i ev = Graph.GetEdgeV(e0);
                int otherv = (ev.a == vid) ? ev.b : ev.a;
                Vector2d newpos = Graph.GetVertex(vid);  //0.5 * (Graph.GetVertex(vid) + Graph.GetVertex(otherv));
                Graph.RemoveEdge(e0, false);
                int newvid = Graph.AppendVertex(newpos);
                Graph.AppendEdge(newvid, otherv);
            }
        }

        private List<int> FindSharpCorners(double sharp_thresh_deg)
        {
            List<int> sharp_corners = new List<int>();
            foreach (int vid in Graph.VertexIndices())
            {
                if (is_fixed_v(vid))
                    continue;
                double open_angle = Graph.OpeningAngle(vid);
                if (open_angle < sharp_thresh_deg)
                    sharp_corners.Add(vid);
            }

            return sharp_corners;
        }

        private static void ResampleGraph(DGraph2Resampler resampler, double overlapRadius, bool resample)
        {
            resampler.CollapseDegenerateEdges(overlapRadius / 10);

            if (!resample)
                return;

            resampler.SplitToMaxEdgeLength(overlapRadius / 2);
            resampler.CollapseToMinEdgeLength(overlapRadius / 3);
            resampler.CollapseDegenerateEdges(overlapRadius / 10);
        }

        /// <summary>
        /// Remove a span of edges that are within a distance threshold from rest of graph.
        /// Walk forward from start_vid along edge first_eid, and keep walking & discarding
        /// until we find a point we want to keep
        /// </summary>
        private void decimate_forward(int start_vid, int first_eid, double dist_thresh)
        {
            int cur_eid = first_eid;
            int cur_vid = start_vid;

            System.Diagnostics.Debug.Assert(Graph.IsEdge(cur_eid));

            bool stop = false;
            while (!stop)
            {
                Index2i nextinfo = DGraph2Util.NextEdgeAndVtx(cur_eid, cur_vid, Graph);
                if (PreserveEdgeFilterF(cur_eid))
                    break;
                edge_hash.RemoveSegmentUnsafe(cur_eid, Graph.GetEdgeCenter(cur_eid));
                Graph.RemoveEdge(cur_eid, true);

                if (nextinfo.a == int.MaxValue)
                    break;

                cur_eid = nextinfo.a;
                cur_vid = nextinfo.b;

                double dist = MinSelfSegDistance(cur_vid, 2 * dist_thresh);
                double collision_dist = MinCollisionConstraintDistance(cur_vid, dist_thresh);
                if (dist > dist_thresh && collision_dist > CollisionRadius)
                    stop = true;
            }
        }

        /// <summary>
        /// Find nearest point to vertex vid in graph, but filter out **connected** neighbourhood within self_radius
        /// </summary>
        private double MinSelfSegDistance(int vid, double self_radius)
        {
            Vector2d pos = Graph.GetVertex(vid);

            List<int> ignore_edges = new List<int>(16);
            FindConnectedEdgesInRadius(vid, self_radius * self_radius, ignore_edges);

            Vector2d a = Vector2d.Zero, b = Vector2d.Zero;
            var result = edge_hash.FindNearestInSquaredRadius(pos, self_radius * self_radius,
                (eid) =>
                {
                    Graph.GetEdgeV(eid, ref a, ref b);
                    return Segment2d.FastDistanceSquared(ref a, ref b, ref pos);
                },
                (eid) => { return ignore_edges.Contains(eid); }
            );

            return (result.Key == -1) ? double.MaxValue : Math.Sqrt(result.Value);
        }

        /// <summary>
        /// find nearest point to vertex in collision graph
        /// </summary>
        private double MinCollisionConstraintDistance(int vid, double collision_radius)
        {
            if (collision_edge_hash == null)
                return double.MaxValue;

            Vector2d pos = Graph.GetVertex(vid);
            Vector2d a = Vector2d.Zero, b = Vector2d.Zero;
            var result = collision_edge_hash.FindNearestInSquaredRadius(pos, collision_radius * collision_radius,
                (eid) =>
                {
                    CollisionGraph.GetEdgeV(eid, ref a, ref b);
                    return Segment2d.FastDistanceSquared(ref a, ref b, ref pos);
                }
            );
            return (result.Key == -1) ? double.MaxValue : Math.Sqrt(result.Value);
        }

        /// <summary>
        /// Starting at vid, find all connected edges where at least one vertex is within euclidean-dist radius
        /// [TODO] move to utility class?
        /// </summary>
        private void FindConnectedEdgesInRadius(int vid, double radius_squared, List<int> edges)
        {
            Vector2d pos = Graph.GetVertex(vid);

            foreach (int eid in Graph.VtxEdgesItr(vid))
            {
                edges.Add(eid);

                Index2i next = new Index2i(eid, vid);
                while (true)
                {
                    next = DGraph2Util.NextEdgeAndVtx(next.a, next.b, Graph);
                    if (next.a == eid)
                        goto looped;   // looped! we can exit now w/o next step of outer loop
                    if (next.a == int.MaxValue)
                        break;
                    edges.Add(next.a);
                    if (pos.DistanceSquared(Graph.GetVertex(next.b)) > radius_squared)
                        break;
                }
            }
            looped:
            return;
        }
    }
}