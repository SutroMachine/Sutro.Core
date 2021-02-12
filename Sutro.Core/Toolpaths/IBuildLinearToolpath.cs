namespace Sutro.Core.Toolpaths
{
    public interface IBuildLinearToolpath<T> : ILinearToolpath<T>
    {
        void ChangeType(ToolpathTypes type);

        void AppendVertex(T v, TPVertexFlags flags);

        void UpdateVertex(int i, T v);

        int VertexCount { get; }
        T Start { get; }
        T End { get; }
    }
}