using Sutro.Core.Settings;

namespace gs
{
    public interface IThreeAxisLaserCompiler
    {
        void Begin();

        void AppendPaths(ToolpathSet paths);

        void End();
    }

    public class SLSCompiler : IThreeAxisLaserCompiler
    {
        private PrintProfileFFF Settings;
        private IPathsAssembler Assembler;

        public SLSCompiler(PrintProfileFFF settings)
        {
            Settings = settings;
        }

        public virtual void Begin()
        {
            Assembler = InitializeAssembler();
        }

        // override to customize assembler
        protected virtual IPathsAssembler InitializeAssembler()
        {
            IPathsAssembler asm = new GenericPathsAssembler();
            return asm;
        }

        public virtual void End()
        {
        }

        public virtual void AppendPaths(ToolpathSet paths)
        {
            Assembler.AppendPaths(paths);
        }

        public ToolpathSet TempGetAssembledPaths()
        {
            return Assembler.TempGetAssembledPaths();
        }
    }
}