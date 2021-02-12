namespace gs
{
    public interface IThreeAxisLaserCompiler
    {
        void Begin();

        void AppendPaths(ToolpathSet paths);

        void End();
    }
}