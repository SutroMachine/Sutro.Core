using g3;
using Sutro.Core.Settings;
using Sutro.Core.Toolpaths;
using System;

namespace Sutro.Core.Compilers
{
    // [TODO] be able to not hardcode this type?

    public interface IThreeAxisMillingCompiler : ICNCCompiler
    {
        // current tool position
        Vector3d ToolPosition { get; }

        // compiler will call this to emit status messages / etc
        Action<string> EmitMessageF { get; set; }

        void Begin();

        void AppendPaths(ToolpathSet paths, IPrintProfileFFF pathSettings);

        void AppendComment(string comment);

        void End();
    }
}