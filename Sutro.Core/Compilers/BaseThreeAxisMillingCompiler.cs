using g3;
using Sutro.Core.Assemblers;
using Sutro.Core.GCodeBuilders;
using Sutro.Core.Settings;
using Sutro.Core.Toolpaths;
using System;

namespace Sutro.Core.Compilers
{
    // [TODO] be able to not hardcode this type?
    using LinearToolpath = LinearToolpath3<PrintVertex>;

    public class BaseThreeAxisMillingCompiler : IThreeAxisMillingCompiler
    {
        public IPrintProfileFFF Settings { get; }
        public GCodeBuilder Builder { get; }
        public BaseMillingAssembler Assembler { get; protected set; }

        private MillingAssemblerFactoryF AssemblerF { get; }

        /// <summary>
        /// compiler will call this to emit status messages / etc
        /// </summary>
        public virtual Action<string> EmitMessageF { get; set; }

        public BaseThreeAxisMillingCompiler(GCodeBuilder builder, IPrintProfileFFF settings, MillingAssemblerFactoryF AssemblerF)
        {
            Builder = builder;
            Settings = settings;
            this.AssemblerF = AssemblerF;
        }

        public Vector3d ToolPosition
        {
            get { return Assembler.ToolPosition; }
        }

        public bool InRetract
        {
            get { return Assembler.InRetract; }
        }

        public bool InTravel
        {
            get { return Assembler.InTravel; }
        }

        public virtual void Begin()
        {
            Assembler = AssemblerF(Builder, Settings);
            Assembler.AppendComment("---BEGIN HEADER");
            Assembler.AppendHeader();
            Assembler.AppendComment("---END HEADER");
        }

        public virtual void End()
        {
            Assembler.UpdateProgress(100);
            Assembler.AppendFooter();
        }

        /// <summary>
        /// Compile this set of toolpaths and pass to assembler.
        /// Settings are optional, pass null to ignore
        /// </summary>
		public virtual void AppendPaths(ToolpathSet paths, IPrintProfileFFF pathSettings)
        {
            int path_index = 0;
            foreach (var gpath in paths)
            {
                path_index++;

                if (IsCommandToolpath(gpath))
                {
                    ProcessCommandToolpath(gpath);
                }
                else
                {
                    ProcessToolpath(gpath, pathSettings);
                }


            }
        }

        protected virtual void ProcessToolpath(IToolpath gpath, IPrintProfileFFF pathSettings)
        {
            LinearToolpath p = gpath as LinearToolpath;
            var useSettings = pathSettings == null ? Settings : pathSettings;


            int i;
            if (p.Type == ToolpathTypes.Travel || p.Type == ToolpathTypes.PlaneChange)
            {
                // do retract cycle
                if (!Assembler.InRetract)
                {
                    Assembler.BeginRetract(useSettings.Part.RetractDistanceMM, useSettings.Part.RetractSpeed, "Retract");
                }
                if (!Assembler.InTravel)
                {
                    Assembler.BeginTravel();
                }
            }
            else if (p.Type == ToolpathTypes.Cut)
            {
                if (Assembler.InTravel)
                    Assembler.EndTravel();

                if (Assembler.InRetract)
                    Assembler.EndRetract(useSettings.Part.RetractSpeed, "End Retract");
            }

            i = 1;      // do not need to emit code for first point of path,
                        // we are already at this pos
            for (; i < p.VertexCount; ++i)
            {
                if (p.Type == ToolpathTypes.Travel)
                {
                    Assembler.AppendMoveTo(p[i].Position, p[i].FeedRate, "Travel");
                }
                else if (p.Type == ToolpathTypes.PlaneChange)
                {
                    Assembler.AppendMoveTo(p[i].Position, p[i].FeedRate, "Plane Change");
                }
                else
                {
                    Assembler.AppendCutTo(p[i].Position, p[i].FeedRate);
                }
            }
        }

        public virtual void AppendComment(string comment)
        {
            Assembler.AppendComment(comment);
        }

        /// <summary>
        /// Command toolpaths are used to pass special commands/etc to the Assembler.
        /// The positions will be ignored
        /// </summary>
        protected virtual bool IsCommandToolpath(IToolpath toolpath)
        {
            return toolpath.Type == ToolpathTypes.Custom
                || toolpath.Type == ToolpathTypes.CustomAssemblerCommands;
        }

        /// <summary>
        /// Called on toolpath if IsCommandToolpath() returns true
        /// </summary>
        protected virtual void ProcessCommandToolpath(IToolpath toolpath)
        {
            if (toolpath.Type == ToolpathTypes.CustomAssemblerCommands)
            {
                AssemblerCommandsToolpath assembler_path = toolpath as AssemblerCommandsToolpath;
                if (assembler_path != null && assembler_path.AssemblerF != null)
                {
                    assembler_path.AssemblerF(Assembler, this);
                }
                else
                {
                    emit_message("ProcessCommandToolpath: invalid " + toolpath.Type.ToString());
                }
            }
            else
            {
                emit_message("ProcessCommandToolpath: unhandled type " + toolpath.Type.ToString());
            }
        }

        protected virtual void emit_message(string text, params object[] args)
        {
            if (EmitMessageF != null)
                EmitMessageF(string.Format(text, args));
        }
    }
}