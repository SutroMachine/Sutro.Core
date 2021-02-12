using System;

namespace Sutro.Core.FunctionalTest.Exceptions
{
    public class CumulativeDistanceException : Exception
    {
        public CumulativeDistanceException(string s) : base(s)
        {
        }
    }
}