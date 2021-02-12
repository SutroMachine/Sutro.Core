using g3;
using System.Collections.Generic;

namespace gs
{
    public static class FillSplitter<TSegmentInfo> where TSegmentInfo : IFillSegment
    {
        public static List<List<FillElement<TSegmentInfo>>> SplitAtDistances(
            IEnumerable<double> splitDistances,
            IEnumerable<FillElement<TSegmentInfo>> elements)
        {
            // TODO: Decide what happens when split distance greater than length.
            // TODO: Check for split distances monotonically increasing and > 0.

            double cumulativeDistance = 0;
            var splitsQueue = new Queue<double>(splitDistances);

            var result = new List<List<FillElement<TSegmentInfo>>>();

            // Initialize the first list of elements
            var splitElements = new List<FillElement<TSegmentInfo>>();

            // If splits are empty, just return the full copy of this curve
            if (splitsQueue.Count == 0)
            {
                splitElements.AddRange(elements);
                result.Add(splitElements);
                return result;
            }

            // If there is a split location on the first vertex, remove first split
            if (MathUtil.EpsilonEqual(splitsQueue.Peek(), 0))
                splitsQueue.Dequeue();

            // Iterate through the fill elements in the polygon.
            foreach (var element in elements)
            {
                // If no splits are left, just add the current point
                if (splitsQueue.Count == 0)
                {
                    splitElements.Add(element);
                    continue;
                }

                // Calculate how much distance the current segment adds
                double nextDistance = element.GetSegment2d().Length;
                var currentElement = element;

                // For each split distance within the current segment
                while (splitsQueue.Count > 0 && splitsQueue.Peek() < cumulativeDistance + nextDistance)
                {
                    // Create normalized split distance (0,1)
                    double splitDistance = splitsQueue.Dequeue() - cumulativeDistance;

                    currentElement.SplitElement(splitDistance / nextDistance, out var splitElementFront, out currentElement);

                    splitElements.Add(splitElementFront);
                    result.Add(splitElements);
                    splitElements = new List<FillElement<TSegmentInfo>>();

                    cumulativeDistance += splitDistance;
                    nextDistance -= splitDistance;
                }

                splitElements.Add(currentElement);

                cumulativeDistance += nextDistance;
            }
            result.Add(splitElements);
            return result;
        }
    }
}