using System;

namespace Sutro.Core.FunctionalTest.Exceptions
{
    public class CumulativeDurationException : Exception
    {
        public CumulativeDurationException(string s) : base(s)
        {
        }
    }
}