using Sutro.Core.Assemblers;
using Sutro.Core.Compilers;
using System;

namespace Sutro.Core.Toolpaths
{
    public class AssemblerCommandsToolpath : SentinelToolpath
    {
        public override ToolpathTypes Type
        {
            get { return ToolpathTypes.CustomAssemblerCommands; }
        }

        public Action<IGCodeAssembler, ICNCCompiler> AssemblerF;
    }
}