namespace gs
{
    public interface ILayerPathsPostProcessor
    {
        void Process(PrintLayerData layerData, ToolpathSet layerPaths);
    }
}