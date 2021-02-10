using g3;
using gs;
using gs.FillTypes;
using Sutro.Core.Models.GCode;
using System.Collections.Generic;

namespace Sutro.Core.Decompilers
{
    public class DecompilerFFF : DecompilerBase<PrintVertex>
    {
        protected override Dictionary<string, IFillType> FillTypes => new Dictionary<string, IFillType>()
        {
            {DefaultFillType.Label, new DefaultFillType() },
            {OuterPerimeterFillType.Label, new OuterPerimeterFillType() },
            {InnerPerimeterFillType.Label, new InnerPerimeterFillType() },
            {OpenShellCurveFillType.Label, new OpenShellCurveFillType()},
            {SolidFillType.Label, new SolidFillType()},
            {SparseFillType.Label, new SparseFillType()},
            {SupportFillType.Label, new SupportFillType()},
            {BridgeFillType.Label, new BridgeFillType()},
        };

        public override void ProcessGCodeLine(GCodeLine line)
        {
            if (LineIsEmpty(line))
                return;

            SetExtrusionCoordinateMode(line);
            currentVertex = UpdatePrintVertex(line, previousVertex);

            if (LineIsNewLayerComment(line, out int index, out double height))
            {
                toolpath = FinishToolpath();
                EmitNewLayer(currentLayerIndex++, height);
            }

            if (LineIsTravel(line))
            {
                CloseToolpathAndAddTravel(previousVertex, currentVertex);
            }

            if (LineIsNewFillTypeComment(line, out var newFillType))
            {
                toolpath = FinishToolpath();
                toolpath.Type = ToolpathTypes.Deposition;
                toolpath.FillType = newFillType;
            }

            if (line.Comment?.Contains("Plane Change") ?? false)
            {
                toolpath.Type = ToolpathTypes.PlaneChange;
            }

            if (line.Type == LineType.GCode)
            {
                if (VertexHasNegativeOrZeroExtrusion(previousVertex, currentVertex))
                {
                    CloseToolpathAndAddTravel(previousVertex, currentVertex);
                }
                else if (toolpath != null)
                {
                    AppendVertexToCurrentToolpath(currentVertex);
                }
            }

            previousVertex = currentVertex;
        }

        private void AppendVertexToCurrentToolpath(PrintVertex vertex)
        {
            if (toolpath.VertexCount == 1)
            {
                var modifiedFirstVertex = new PrintVertex(toolpath.StartPosition, vertex.FeedRate, vertex.Dimensions, vertex.Extrusion.x);
                modifiedFirstVertex.ExtendedData = vertex.ExtendedData;
                modifiedFirstVertex.Source = vertex.Source;
                toolpath.UpdateVertex(0, modifiedFirstVertex);
            }
            toolpath.AppendVertex(vertex, TPVertexFlags.None);

        }

        private void CloseToolpathAndAddTravel(PrintVertex previousVertex, PrintVertex currentVertex)
        {
            toolpath = FinishToolpath();

            if (currentVertex == null)
                return;

            CreateTravelToolpath(previousVertex, currentVertex);
            toolpath = StartNewToolpath(toolpath, currentVertex);
        }

        private bool VertexHasNegativeOrZeroExtrusion(PrintVertex previousVertex, PrintVertex currentVertex)
        {
            if (previousVertex == null || currentVertex == null)
                return false;

            if (extruderRelativeCoordinates)
            {
                // TODO: Update this for relative vs. absolute extruder coords
                return currentVertex.Extrusion.x <= 0;
            }
            else
            {
                return currentVertex.Extrusion.x <= previousVertex.Extrusion.x;
            }
        }

        protected override LinearToolpath3<PrintVertex> FinishToolpath()
        {
            if (toolpath == null)
                return new LinearToolpath3<PrintVertex>();

            toolpath.Type = SetTypeFromVertexExtrusions(toolpath);
            EmitToolpath(toolpath);

            // TODO: Simplify with "CopyProperties" method on LinearToolpath3
            var newToolpath = new LinearToolpath3<PrintVertex>(toolpath.Type);
            newToolpath.IsHole = toolpath.IsHole;
            newToolpath.FillType = toolpath.FillType;
            newToolpath.AppendVertex(toolpath.VertexCount > 0 ? toolpath.End : currentVertex, TPVertexFlags.IsPathStart);

            return newToolpath;
        }

        private ToolpathTypes SetTypeFromVertexExtrusions(LinearToolpath3<PrintVertex> toolpath)
        {
            for (int i = 1; i < toolpath.VertexCount; i++)
            {
                if (!VertexHasNegativeOrZeroExtrusion(toolpath[i - 1], toolpath[i]))
                    return ToolpathTypes.Deposition;
            }
            return ToolpathTypes.Travel;
        }

        private PrintVertex UpdatePrintVertex(GCodeLine line, PrintVertex previousVertex)
        {
            return new PrintVertex(
                ExtractPosition(line, previousVertex.Position),
                ExtractFeedRate(line, previousVertex.FeedRate),
                ExtractDimensions(line, previousVertex.Dimensions),
                ExtractExtrusion(line, previousVertex.Extrusion.x));
        }

        protected override PrintVertex CreateDefaultVertex()
        {
            return new PrintVertex(Vector3d.Zero, 0, Vector2d.Zero);
        }
    }
}