﻿using g3;
using Sutro.Core.Models.GCode;
using Sutro.Core.Parsers;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Sutro.Core.Interpreters
{
    /// <summary>
    /// Basic 3-axis CNC interpreter
    /// </summary>
    public class ThreeAxisCNCInterpreter : IGCodeInterpreter
    {
        private IGCodeListener listener = null;

        private Dictionary<int, Action<GCodeLine>> GCodeMap = new Dictionary<int, Action<GCodeLine>>();
        private Dictionary<int, Action<GCodeLine>> MCodeMap = new Dictionary<int, Action<GCodeLine>>();

        private bool UseRelativePosition = false;

        private Vector3d CurPosition = Vector3d.Zero;

        private bool in_travel = false;
        private bool in_cut = false;

        public ThreeAxisCNCInterpreter()
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
                args.HasTypeFilter ? file.AllLines() : file.AllLinesOfType(args.eTypeFilter);

            listener.Begin();

            CurPosition = Vector3d.Zero;

            foreach (GCodeLine line in lines_enum)
            {
                Action<GCodeLine> parseF;
                if (line.Type == LineType.GCode)
                {
                    if (GCodeMap.TryGetValue(line.Code, out parseF))
                        parseF(line);
                }
                else if (line.Type == LineType.MCode)
                {
                    if (MCodeMap.TryGetValue(line.Code, out parseF))
                        parseF(line);
                }
            }

            listener.End();
        }

        public virtual IEnumerable<bool> InterpretInteractive(GCodeFile file, InterpretArgs args)
        {
            IEnumerable<GCodeLine> lines_enum =
                args.HasTypeFilter ? file.AllLinesOfType(args.eTypeFilter) : file.AllLines();

            listener.Begin();

            CurPosition = Vector3d.Zero;

            foreach (GCodeLine line in lines_enum)
            {
                if (line.Type == LineType.GCode)
                {
                    Action<GCodeLine> parseF;
                    if (GCodeMap.TryGetValue(line.Code, out parseF))
                    {
                        parseF(line);
                        yield return true;
                    }
                }
            }

            listener.End();

            yield return false;
        }

        private void emit_linear(GCodeLine line)
        {
            Debug.Assert(line.Code == 0 || line.Code == 1);

            double x = GCodeUtil.UnspecifiedValue,
                y = GCodeUtil.UnspecifiedValue,
                z = GCodeUtil.UnspecifiedValue;
            bool found_x = GCodeUtil.TryFindParamNum(line.Parameters, "X", ref x);
            bool found_y = GCodeUtil.TryFindParamNum(line.Parameters, "Y", ref y);
            bool found_z = GCodeUtil.TryFindParamNum(line.Parameters, "Z", ref z);
            Vector3d newPos = UseRelativePosition ? Vector3d.Zero : CurPosition;
            if (found_x)
                newPos.x = x;
            if (found_y)
                newPos.y = y;
            if (found_z)
                newPos.z = z;
            if (UseRelativePosition)
                CurPosition += newPos;
            else
                CurPosition = newPos;

            // F is feed rate (this changes?)
            double f = 0;
            bool haveF = GCodeUtil.TryFindParamNum(line.Parameters, "F", ref f);

            LinearMoveData move = new LinearMoveData(
                newPos,
                haveF ? f : GCodeUtil.UnspecifiedValue);

            bool is_travel = line.Code == 0;

            if (is_travel)
            {
                if (in_travel == false)
                {
                    listener.BeginTravel();
                    in_travel = true;
                    in_cut = false;
                }
            }
            else
            {
                if (in_cut == false)
                {
                    listener.BeginCut();
                    in_travel = false;
                    in_cut = true;
                }
            }

            move.source = line;
            Debug.Assert(in_travel || in_cut);
            listener.LinearMoveToAbsolute3d(move);
        }

        // G92 - Position register: Set the specified axes positions to the given position
        // Sets the position of the state machine and the bot. NB: There are two methods of forming the G92 command:
        private void set_position(GCodeLine line)
        {
            double x = 0, y = 0, z = 0;
            if (GCodeUtil.TryFindParamNum(line.Parameters, "X", ref x))
            {
                CurPosition.x = x;
            }
            if (GCodeUtil.TryFindParamNum(line.Parameters, "Y", ref y))
            {
                CurPosition.y = y;
            }
            if (GCodeUtil.TryFindParamNum(line.Parameters, "Z", ref z))
            {
                CurPosition.z = z;
            }
        }

        // G90
        private void set_absolute_positioning(GCodeLine line)
        {
            UseRelativePosition = false;
        }

        // G91
        private void set_relative_positioning(GCodeLine line)
        {
            UseRelativePosition = true;
        }

        private void build_maps()
        {
            // G0 = rapid move
            GCodeMap[0] = emit_linear;

            // G1 = linear move
            GCodeMap[1] = emit_linear;

            // G4 = CCW circular
            //GCodeMap[4] = emit_ccw_arc;
            //GCodeMap[5] = emit_cw_arc;

            GCodeMap[90] = set_absolute_positioning;    // http://reprap.org/wiki/G-code#G90:_Set_to_Absolute_Positioning
            GCodeMap[91] = set_relative_positioning;    // http://reprap.org/wiki/G-code#G91:_Set_to_Relative_Positioning
            GCodeMap[92] = set_position;                // http://reprap.org/wiki/G-code#G92:_Set_Position
        }
    }
}