using Sutro.Core.Generators;
using Sutro.Core.Toolpaths;

namespace Sutro.Core.Toolpathing
{
    public interface ILayerPathsPostProcessor
    {
        void Process(PrintLayerData layerData, ToolpathSet layerPaths);
    }
}