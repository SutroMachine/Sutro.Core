using g3;
using Sutro.Core.Models;
using System;

namespace gs
{
    public static class MachineBedOriginLocationUtility
    {
        public static MachineBedOriginLocationX LocationXFromScalar(double x)
        {
            if (MathUtil.EpsilonEqual(x, 0))
                return MachineBedOriginLocationX.Left;
            else if (MathUtil.EpsilonEqual(x, 0.5))
                return MachineBedOriginLocationX.Center;
            else if (MathUtil.EpsilonEqual(x, 1))
                return MachineBedOriginLocationX.Right;
            else
                throw new ArgumentException($"Can't convert value {x} to MachineBedOriginLocationX");
        }

        public static MachineBedOriginLocationY LocationYFromScalar(double y)
        {
            if (MathUtil.EpsilonEqual(y, 0))
                return MachineBedOriginLocationY.Front;
            else if (MathUtil.EpsilonEqual(y, 0.5))
                return MachineBedOriginLocationY.Center;
            else if (MathUtil.EpsilonEqual(y, 1))
                return MachineBedOriginLocationY.Back;
            else
                throw new ArgumentException($"Can't convert value {y} to MachineBedOriginLocationY");
        }

        public static double LocationXFromEnum(MachineBedOriginLocationX location)
        {
            switch (location)
            {
                case MachineBedOriginLocationX.Left:
                    return 0;

                case MachineBedOriginLocationX.Center:
                    return 0.5;

                case MachineBedOriginLocationX.Right:
                    return 1;
            }
            return 0.5;
        }

        public static double LocationYFromEnum(MachineBedOriginLocationY location)
        {
            switch (location)
            {
                case MachineBedOriginLocationY.Front:
                    return 0;

                case MachineBedOriginLocationY.Center:
                    return 0.5;

                case MachineBedOriginLocationY.Back:
                    return 1;
            }
            return 0.5;
        }
    }
}