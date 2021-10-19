using g3;
using System;
using System.Collections.Generic;

namespace gs
{
    // TODO: Move this to geometry3Sharp when convenient
    public static class DGraph3UtilExtensions
    {
        /// <summary>
        /// Combines vertices that are very close to the same location; useful
        /// for handling floating-point math errors.
        /// </summary>
        public static void JoinNearbyBoundaryVertices(this DGraph3 graph, double weldTolerance)
        {
            // Find all boundary vertices
            var boundaries = new List<int>();
            foreach (int vid in graph.VertexIndices())
            {
                if (graph.IsBoundaryVertex(vid))
                {
                    boundaries.Add(vid);
                }
            }

            var sets = FindMatchingVertices(graph, weldTolerance, boundaries);

            foreach(var set in sets)
            {
                for (int i = 1; i < set.Count; i++)
                {
                    ReplaceVertex(graph, set[0], set[i]);
                }
            }

        }

        private static void ReplaceVertex(DGraph3 graph, int vidReplacement, int vidOriginal)
        {
            List<int> edges = new List<int>(graph.GetVtxEdges(vidOriginal));
            foreach (int eid in edges)
            {
                var edge = graph.GetEdge(eid);
                MeshResult result = graph.RemoveEdge(eid, true);
                if (result != MeshResult.Ok)
                    continue;

                int vidOther = edge.a == vidReplacement ? edge.b : edge.a;
                var eidExisting = graph.FindEdge(vidReplacement, vidOther);
                if (eidExisting < 0)
                {
                    graph.AppendEdge(vidReplacement, vidOther, edge.c);
                }
            }
        }

        private static List<List<int>> FindMatchingVertices(DGraph3 graph, double weldTolerance, List<int> boundaries)
        {
            var combineSets = new List<List<int>>();

            foreach (int vid in boundaries)
            {
                bool matchFound = false;
                Vector3d vec = graph.GetVertex(vid);
                foreach (var set in combineSets)
                {
                    foreach (int i in set)
                    {
                        if (vec.EpsilonEqual(graph.GetVertex(i), weldTolerance))
                        {
                            matchFound = true;
                            break;
                        }
                    }

                    if (matchFound)
                    {
                        set.Add(vid);
                        break;
                    }
                }

                if (!matchFound)
                {
                    combineSets.Add(new List<int>() { vid });
                }
            }

            return combineSets;
        }
    }
}