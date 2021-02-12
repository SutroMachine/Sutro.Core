using System;

namespace Sutro.Core.Test.Exceptions
{
    public class CenterOfMassException : Exception
    {
        public CenterOfMassException(string s) : base(s)
        {
        }
    }
}