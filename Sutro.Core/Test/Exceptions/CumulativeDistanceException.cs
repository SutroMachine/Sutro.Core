using System;

namespace Sutro.Core.Test.Exceptions
{
    public class CumulativeDistanceException : Exception
    {
        public CumulativeDistanceException(string s) : base(s)
        {
        }
    }
}