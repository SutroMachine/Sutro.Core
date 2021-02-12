using System;

namespace Sutro.Core.Test.Exceptions
{
    public class BoundingBoxException : Exception
    {
        public BoundingBoxException(string s) : base(s)
        {
        }
    }
}