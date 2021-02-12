using g3;
using Sutro.Core.Models.GCode;
using System;

namespace gs
{
    static public class GCodeUtil
    {
        public const double UnspecifiedValue = double.MaxValue;
        public static readonly Vector3d UnspecifiedPosition = Vector3d.MaxValue;
        public static readonly Vector2d UnspecifiedDimensions = Vector2d.MaxValue;

        public static Vector3d Extrude(double a)
        {
            return new Vector3d(a, UnspecifiedValue, UnspecifiedValue);
        }

        // returns index of param, or -1
        static public int TryFindParam(GCodeParam[] paramList, string identifier)
        {
            for (int i = 0; i < paramList.Length; ++i)
                if (paramList[i].Identifier == identifier)
                    return i;
            return -1;
        }

        static public bool TryFindParamNum(GCodeParam[] paramList, string identifier, ref double d)
        {
            if (paramList == null)
                return false;
            for (int i = 0; i < paramList.Length; ++i)
            {
                if (paramList[i].Identifier == identifier)
                {
                    if (paramList[i].Type == GCodeParamTypes.DoubleValue)
                    {
                        d = paramList[i].DoubleValue;
                        return true;
                    }
                    else if (paramList[i].Type == GCodeParamTypes.IntegerValue)
                    {
                        d = paramList[i].IntegerValue;
                        return true;
                    }
                    else
                        return false;
                }
            }
            return false;
        }

        public enum NumberType
        {
            Integer, Decimal, NotANumber
        }

        // doesn't handle e^ numbers
        // doesn't allow commas
        static public NumberType GetNumberType(string s)
        {
            int N = s.Length;

            bool saw_digit = false;
            bool saw_dot = false;
            bool saw_sign = false;
            for (int i = 0; i < N; ++i)
            {
                char c = s[i];
                if (c == '-')
                {
                    if (saw_digit || saw_dot || saw_sign)
                        return NumberType.NotANumber;
                    saw_sign = true;
                }
                else if (c == '.')
                {
                    if (saw_dot)
                        return NumberType.NotANumber;
                    saw_dot = true;
                }
                else if (Char.IsDigit(c))
                {
                    saw_digit = true;
                }
                else
                {
                    return NumberType.NotANumber;
                }
            }
            if (!saw_digit)
                return NumberType.NotANumber;
            return (saw_dot) ? NumberType.Decimal : NumberType.Integer;
        }
    }
}