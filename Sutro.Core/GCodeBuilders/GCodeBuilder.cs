using Sutro.Core.Models.GCode;
using System;
using System.Collections.Generic;

namespace Sutro.Core.GCodeBuilders
{
    public class GCodeBuilder
    {
        public virtual IGCodeAccumulator Target { get; }

        public GCodeBuilder(IGCodeAccumulator target)
        {
            Target = target;
        }

        public virtual GCodeBuilder AddLine(GCodeLine line, bool bClone = true)
        {
            close_current_line();
            if (bClone)
            {
                GCodeLine clone = line.Clone();
                clone.LineNumber = next_line_number();
                Target.AddLine(clone);
            }
            else
            {
                line.LineNumber = next_line_number();
                Target.AddLine(line);
            }
            return this;
        }

        /// <summary>
        /// Add a comment to output. Will add necessary comment prefix if not provided.
        /// </summary>
        public virtual GCodeBuilder AddCommentLine(string comment)
        {
            //if ( comment[0] != ';' && comment[0] != '(' )
            //	comment = ";" + comment;
            AddLine(
                new GCodeLine(next_line_number(), LineType.Comment, comment));
            return this;
        }

        /// <summary>
        /// Add this exact string to output. not a good idea.
        /// </summary>
        public virtual GCodeBuilder AddExplicitLine(string line)
        {
            AddLine(
                new GCodeLine(next_line_number(), LineType.UnknownString, line));
            return this;
        }

        /// <summary>
        /// Add empty line to output.
        /// </summary>
        public virtual GCodeBuilder AddBlankLine()
        {
            AddLine(
                new GCodeLine(next_line_number(), LineType.Blank));
            return this;
        }

        /// <summary>
        /// Open a G code line. Use AppendXParameter to add more info
        /// </summary>
        public virtual GCodeBuilder BeginGLine(int Gcode, string comment = null)
        {
            begin_new_line(LineType.GCode);
            next_line.Code = Gcode;
            if (comment != null)
                next_line.Comment = comment;
            return this;
        }

        /// <summary>
        /// Open a M code line. Use AppendXParameter to add more info
        /// </summary>
        public virtual GCodeBuilder BeginMLine(int Mcode, string comment = null)
        {
            begin_new_line(LineType.MCode);
            next_line.Code = Mcode;
            if (comment != null)
                next_line.Comment = comment;
            return this;
        }

        /// <summary>
        /// close and append current line. In most cases this does not have to be explicitly called.
        /// </summary>
        public virtual GCodeBuilder EndLine()
        {
            close_current_line();
            return this;
        }

        public virtual GCodeBuilder AppendComment(string comment)
        {
            if (next_line == null)
                throw new Exception("GCodeBuilder.AppendComment: next_line was null!?");
            next_line.Comment = comment;
            return this;
        }

        /// <summary>
        /// Add an integer-value parameter to the current line
        /// </summary>
        public virtual GCodeBuilder AppendI(string identifier, int value)
        {
            GCodeParam p = GCodeParam.Integer(value, identifier);
            next_params.Add(p);
            return this;
        }

        /// <summary>
        /// Add a float-value parameter to the current line
        /// </summary>
        public virtual GCodeBuilder AppendF(string identifier, double value)
        {
            GCodeParam p = GCodeParam.Double(value, identifier);
            next_params.Add(p);
            return this;
        }

        /// <summary>
        /// Add a text-value parameter to the current line
        /// (these are used in Biesse gcode...can also use this to string-format values yourself)
        /// </summary>
        public virtual GCodeBuilder AppendS(string identifier, string value)
        {
            GCodeParam p = GCodeParam.Text(value, identifier);
            next_params.Add(p);
            return this;
        }

        /// <summary>
        /// Append a label-only parameter to the current line (eg like G162 X Y)
        /// </summary>
        /// <param name="identifier">Identifier.</param>
        public virtual GCodeBuilder AppendL(string identifier)
        {
            GCodeParam p = GCodeParam.NoValue(identifier);
            next_params.Add(p);
            return this;
        }

        protected int line_number = 0;

        protected virtual int next_line_number()
        {
            return line_number++;
        }

        protected GCodeLine next_line;
        protected List<GCodeParam> next_params;

        protected virtual void close_current_line()
        {
            if (next_line != null)
            {
                if (next_params.Count > 0)
                    next_line.Parameters = next_params.ToArray();
                Target.AddLine(next_line);
                next_line = null;
            }
        }

        protected virtual void begin_new_line(LineType type)
        {
            if (next_line != null)
                close_current_line();

            next_line = new GCodeLine(next_line_number(), type);
            if (next_params == null || next_params.Count > 0)
                next_params = new List<GCodeParam>();
        }
    }
}