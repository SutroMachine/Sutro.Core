using System.Collections.Generic;

namespace gs
{
    public class LayerPathsPostProcessorSequence : ILayerPathsPostProcessor
    {
        public List<ILayerPathsPostProcessor> Posts = new List<ILayerPathsPostProcessor>();

        public virtual void Process(PrintLayerData layerData, ToolpathSet layerPaths)
        {
            foreach (var post in Posts)
                post.Process(layerData, layerPaths);
        }
    }
}