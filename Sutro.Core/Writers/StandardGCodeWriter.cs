using Sutro.Core.Models.GCode;
using System;
using System.IO;
using System.Text;

namespace gs
{
    public class StandardGCodeWriter : GCodeWriterBase
    {
        private int float_precision = 5;
        private string float_format = "{0:0.#####}";

        public int FloatPrecision
        {
            get { return float_precision; }
            set
            {
                float_precision = value;
                float_format = "{0:0." + new String('#', float_precision) + "}";
            }
        }

        public enum CommentStyles
        {
            Semicolon = 0,
            Bracket = 1
        }

        public CommentStyles CommentStyle = CommentStyles.Semicolon;

        public override void WriteLine(GCodeLine line, TextWriter outStream)
        {
            if (line.Type == LineType.Comment)
            {
                if (CommentStyle == CommentStyles.Semicolon)
                {
                    if (line.Comment[0] != ';')
                        outStream.Write(";");
                    outStream.WriteLine(line.Comment);
                }
                else
                {
                    outStream.Write("(");
                    outStream.Write(line.Comment);
                    outStream.WriteLine(")");
                }
                return;
            }
            else if (line.Type == LineType.UnknownString)
            {
                outStream.WriteLine(line.OriginalString);
                return;
            }
            else if (line.Type == LineType.Blank)
            {
                outStream.WriteLine();
                return;
            }

            StringBuilder b = new StringBuilder();
            if (line.Type == LineType.MCode)
            {
                b.Append('M');
            }
            else if (line.Type == LineType.GCode)
            {
                b.Append('G');
            }
            else
            {
                throw new Exception("StandardGCodeWriter.WriteLine: unsupported line type");
            }

            b.Append(line.Code);
            b.Append(' ');

            if (line.Parameters != null)
            {
                foreach (GCodeParam p in line.Parameters)
                {
                    if (p.Type == GCodeParamTypes.Code)
                    {
                        //
                    }
                    else if (p.Type == GCodeParamTypes.IntegerValue)
                    {
                        b.Append(p.Identifier);
                        b.Append(p.IntegerValue);
                        b.Append(' ');
                    }
                    else if (p.Type == GCodeParamTypes.DoubleValue)
                    {
                        b.Append(p.Identifier);
                        b.AppendFormat(float_format, p.DoubleValue);
                        b.Append(' ');
                    }
                    else if (p.Type == GCodeParamTypes.TextValue)
                    {
                        b.Append(p.Identifier);
                        b.Append(p.TextValue);
                        b.Append(' ');
                    }
                    else if (p.Type == GCodeParamTypes.NoValue)
                    {
                        b.Append(p.Identifier);
                        b.Append(' ');
                    }
                    else
                    {
                        throw new Exception("StandardGCodeWriter.WriteLine: unsupported parameter type");
                    }
                }
            }

            if (line.Comment != null && line.Comment.Length > 0)
            {
                if (CommentStyle == CommentStyles.Semicolon)
                {
                    if (line.Comment[0] != '(' && line.Comment[0] != ';')
                        b.Append(';');
                    b.Append(line.Comment);
                }
                else
                {
                    b.Append("(");
                    b.Append(line.Comment);
                    b.Append(")");
                }
            }

            outStream.WriteLine(b.ToString());
        }
    }
}