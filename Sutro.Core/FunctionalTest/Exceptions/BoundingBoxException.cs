using System;

namespace Sutro.Core.FunctionalTest.Exceptions
{
    public class BoundingBoxException : Exception
    {
        public BoundingBoxException(string s) : base(s)
        {
        }
    }
}