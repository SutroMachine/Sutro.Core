using g3;
using Sutro.Core.Models.GCode;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace gs
{
    public class BiesseInterpreter : IGCodeInterpreter
    {
        private IGCodeListener listener = null;
        private Dictionary<int, Action<GCodeLine>> GCodeMap = new Dictionary<int, Action<GCodeLine>>();

        public BiesseInterpreter()
        {
            build_maps();
        }

        public virtual void AddListener(IGCodeListener listener)
        {
            if (this.listener != null)
                throw new Exception("Only one listener supported!");
            this.listener = listener;
        }

        public virtual void Interpret(GCodeFile file, InterpretArgs args)
        {
            IEnumerable<GCodeLine> lines_enum =
                (args.HasTypeFilter) ? file.AllLines() : file.AllLinesOfType(args.eTypeFilter);

            listener.Begin();

            foreach (GCodeLine line in lines_enum)
            {
                if (line.Type == LineType.GCode)
                {
                    Action<GCodeLine> parseF;
                    if (GCodeMap.TryGetValue(line.Code, out parseF))
                        parseF(line);
                }
            }

            listener.End();
        }

        private void emit_linear(GCodeLine line)
        {
            Debug.Assert(line.Code == 1);

            double dx = 0, dy = 0;
            bool brelx = GCodeUtil.TryFindParamNum(line.Parameters, "XI", ref dx);
            bool brely = GCodeUtil.TryFindParamNum(line.Parameters, "YI", ref dy);

            LinearMoveData move = new LinearMoveData(new Vector2d(dx, dy));

            if (brelx || brely)
            {
                listener.LinearMoveToRelative2d(move);
                return;
            }

            double x = 0, y = 0;
            bool absx = GCodeUtil.TryFindParamNum(line.Parameters, "X", ref x);
            bool absy = GCodeUtil.TryFindParamNum(line.Parameters, "Y", ref y);
            if (absx && absy)
            {
                listener.LinearMoveToAbsolute2d(move);
                return;
            }

            // [RMS] can we have this??
            if (absx || absy)
                System.Diagnostics.Debug.Assert(false);
        }

        private void emit_cw_arc(GCodeLine line)
        {
            emit_arc(line, true);
        }

        private void emit_ccw_arc(GCodeLine line)
        {
            emit_arc(line, false);
        }

        private void emit_arc(GCodeLine line, bool clockwise)
        {
            double dx = 0, dy = 0;

            // either of these might be missing...
            bool brelx = GCodeUtil.TryFindParamNum(line.Parameters, "XI", ref dx);
            bool brely = GCodeUtil.TryFindParamNum(line.Parameters, "YI", ref dy);
            Debug.Assert(brelx == true && brely == true);

            double r = 0;
            bool br = GCodeUtil.TryFindParamNum(line.Parameters, "R", ref r);
            Debug.Assert(br == true);

            // [RMS] seems like G5 always has negative radius and G4 positive ??
            //   (this will tell us)
            Debug.Assert((clockwise && r < 0) || (clockwise == false && r > 0));
            r = Math.Abs(r);

            listener.ArcToRelative2d(new Vector2d(dx, dy), r, clockwise);
        }

        private void build_maps()
        {
            // G1 = linear move
            GCodeMap[1] = emit_linear;

            // G4 = CCW circular
            GCodeMap[4] = emit_ccw_arc;
            GCodeMap[5] = emit_cw_arc;
        }
    }
}