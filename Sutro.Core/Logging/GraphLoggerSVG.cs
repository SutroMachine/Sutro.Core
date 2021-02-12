using g3;
using System.IO;

namespace gs
{
    public class GraphLoggerSVG : IGraphLogger
    {
        private readonly string logDirectory;

        public GraphLoggerSVG(string logDirectory = "")
        {
            this.logDirectory = logDirectory;
        }

        public void LogGraph(DGraph2 graph, string identifier)
        {
            SVGWriter svg = new SVGWriter();
            svg.AddGraph(graph, SVGWriter.Style.Outline("black", 0.05f));
            foreach (int vid in graph.VertexIndices())
            {
                if (graph.IsJunctionVertex(vid))
                    svg.AddCircle(new Circle2d(graph.GetVertex(vid), 0.25f), SVGWriter.Style.Outline("red", 0.1f));
                else if (graph.IsBoundaryVertex(vid))
                    svg.AddCircle(new Circle2d(graph.GetVertex(vid), 0.25f), SVGWriter.Style.Outline("blue", 0.1f));
            }
            svg.Write(Path.Combine(logDirectory, $"{identifier}.svg"));
        }
    }
}