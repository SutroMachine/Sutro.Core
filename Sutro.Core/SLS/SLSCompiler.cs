using Sutro.Core.Settings;
using Sutro.Core.Toolpaths;

namespace Sutro.Core.SLS
{
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