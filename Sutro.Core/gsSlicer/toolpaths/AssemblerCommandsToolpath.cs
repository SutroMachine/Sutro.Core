using System;

namespace gs
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