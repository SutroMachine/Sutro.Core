using System;

namespace Sutro.Core.Test.Exceptions
{
    public class CumulativeDurationException : Exception
    {
        public CumulativeDurationException(string s) : base(s)
        {
        }
    }
}