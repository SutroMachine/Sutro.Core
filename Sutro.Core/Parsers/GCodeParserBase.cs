using Sutro.Core.Models.GCode;
using System.IO;

namespace gs
{
    /// <summary>
    /// Base class to reconstruct a GCodeFile object from text.
    /// </summary>
    public abstract class GCodeParserBase
    {
        /// <summary>
        /// Reconstruct a GCodeFile object from text input.
        /// </summary>
        public GCodeFile Parse(TextReader input)
        {
            GCodeFile file = new GCodeFile();

            int lines = 0;
            while (input.Peek() >= 0)
            {
                string line = input.ReadLine();
                int nLineNum = lines++;

                GCodeLine l = ParseLine(line, nLineNum);
                file.AppendLine(l);
            }

            return file;
        }

        /// <summary>
        /// Parse a single line of the GCode file; this must be implemented
        /// in the concrete class.
        /// </summary>
        public abstract GCodeLine ParseLine(string line, int lineNumber);
    }
}