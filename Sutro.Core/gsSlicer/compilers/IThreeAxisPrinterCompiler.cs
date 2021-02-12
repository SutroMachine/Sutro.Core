using g3;
using Sutro.Core.Settings;
using System;
using System.Collections.Generic;

namespace gs
{
    // [TODO] be able to not hardcode this type?
    public interface IThreeAxisPrinterCompiler : ICNCCompiler
    {
        // current nozzle position
        Vector3d NozzlePosition { get; }

        // compiler will call this to emit status messages / etc
        Action<string> EmitMessageF { get; set; }

        void Begin();

        void AppendPaths(ToolpathSet paths, IPrintProfileFFF pathSettings);

        void AppendComment(string comment);

        void End();

        void AppendBlankLine();

        IEnumerable<string> GenerateTotalExtrusionReport(IPrintProfileFFF settings);
    }
}