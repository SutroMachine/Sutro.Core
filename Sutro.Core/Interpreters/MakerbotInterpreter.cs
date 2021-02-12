using g3;
using Sutro.Core.Models.GCode;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace gs
{
    // useful documents:
    //   https://github.com/makerbot/s3g/blob/master/doc/GCodeProtocol.md

    /// <summary>
    /// Makerbot GCode interpreter.
    /// </summary>
    public class MakerbotInterpreter : IGCodeInterpreter
    {
        protected IGCodeListener listener = null;

        protected Dictionary<int, Action<GCodeLine>> GCodeMap = new Dictionary<int, Action<GCodeLine>>();
        protected Dictionary<int, Action<GCodeLine>> MCodeMap = new Dictionary<int, Action<GCodeLine>>();

        protected bool UseRelativePosition = false;
        protected bool UseRelativeExtruder = false;

        protected Vector3d CurPosition = Vector3d.Zero;

        protected double ExtrusionA = 0;
        protected double LastRetractA = 0;
        protected bool in_retract = false;
        protected bool in_travel = false;
        protected bool in_extrude = false;

        public MakerbotInterpreter()
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

            ExtrusionA = 0;
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
                (args.HasTypeFilter) ? file.AllLinesOfType(args.eTypeFilter) : file.AllLines();

            listener.Begin();

            ExtrusionA = 0;
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
            EmitLinear(line);
        }

        protected virtual void EmitLinear(GCodeLine line)
        {
            Debug.Assert(line.Code == 0 || line.Code == 1);

            double x = GCodeUtil.UnspecifiedValue,
                y = GCodeUtil.UnspecifiedValue,
                z = GCodeUtil.UnspecifiedValue;
            bool found_x = GCodeUtil.TryFindParamNum(line.Parameters, "X", ref x);
            bool found_y = GCodeUtil.TryFindParamNum(line.Parameters, "Y", ref y);
            bool found_z = GCodeUtil.TryFindParamNum(line.Parameters, "Z", ref z);
            Vector3d newPos = (UseRelativePosition) ? Vector3d.Zero : CurPosition;
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

            // A is extrusion stepper. E is also "current" stepper.
            double a = 0;
            bool haveA = GCodeUtil.TryFindParamNum(line.Parameters, "A", ref a);
            if (haveA == false)
            {
                haveA = GCodeUtil.TryFindParamNum(line.Parameters, "E", ref a);
            }
            if (UseRelativeExtruder)
                a = ExtrusionA + a;

            LinearMoveData move = new LinearMoveData(
                newPos,
                (haveF) ? f : GCodeUtil.UnspecifiedValue,
                (haveA) ? GCodeUtil.Extrude(a) : GCodeUtil.UnspecifiedPosition);

            if (haveA == false)
            {
                // if we do not have extrusion, this is a travel move
                if (in_travel == false)
                {
                    listener.BeginTravel();
                    in_travel = true;
                    in_extrude = false;
                }
            }
            else if (in_retract)
            {
                // if we are in retract, we stay in until we see forward movement

                Debug.Assert(in_travel);
                Debug.Assert(a <= LastRetractA + 0.001);
                if (MathUtil.EpsilonEqual(a, LastRetractA, 0.00001))
                {
                    in_retract = false;
                    listener.BeginDeposition();
                    in_extrude = true;
                    in_travel = false;
                    ExtrusionA = a;
                }
            }
            else if (a < ExtrusionA)
            {
                // if extrusion moved backwards, we need to enter travel

                in_retract = true;
                LastRetractA = ExtrusionA;
                ExtrusionA = a;
                if (in_travel == false)
                {
                    listener.BeginTravel();
                    in_travel = true;
                    in_extrude = false;
                }
            }
            else
            {
                // if we are in travel, we need to begin extruding
                if (in_travel)
                {
                    listener.BeginDeposition();
                    in_travel = false;
                    in_extrude = true;
                }
                if (in_extrude == false)
                {       // handle initialization cases
                    listener.BeginDeposition();
                    in_extrude = true;
                }
                ExtrusionA = a;
            }

            move.source = line;
            Debug.Assert(in_travel || in_extrude);
            listener.LinearMoveToAbsolute3d(move);
        }

        // G92 - Position register: Set the specified axes positions to the given position
        // Sets the position of the state machine and the bot. NB: There are two methods of forming the G92 command:
        private void set_position(GCodeLine line)
        {
            double x = 0, y = 0, z = 0, a = 0;
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
            if (GCodeUtil.TryFindParamNum(line.Parameters, "A", ref a))
            {
                ExtrusionA = a;
                listener.CustomCommand(
                    (int)CustomListenerCommands.ResetExtruder, GCodeUtil.Extrude(a));
                // reset our state
                in_travel = in_extrude = in_retract = false;
            }

            // E is "current" stepper (A for single extruder)
            double e = 0;
            if (GCodeUtil.TryFindParamNum(line.Parameters, "E", ref e))
            {
                ExtrusionA = e;
                listener.CustomCommand(
                    (int)CustomListenerCommands.ResetExtruder, GCodeUtil.Extrude(e));
                // reset our state
                in_travel = in_extrude = in_retract = false;
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

            // [RMS] according to http://reprap.org/wiki/G-code#G91:_Set_to_Relative_Positioning,
            //   this should only happen on some firmware...
            UseRelativeExtruder = true;
        }

        // M82
        private void set_absolute_extruder(GCodeLine line)
        {
            UseRelativeExtruder = false;
        }

        // M83
        private void set_relative_extruder(GCodeLine line)
        {
            UseRelativeExtruder = true;
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

            MCodeMap[82] = set_absolute_extruder;       // http://reprap.org/wiki/G-code#M83:_Set_extruder_to_relative_mode
            MCodeMap[83] = set_relative_extruder;       // http://reprap.org/wiki/G-code#M83:_Set_extruder_to_relative_mode
        }
    }
}