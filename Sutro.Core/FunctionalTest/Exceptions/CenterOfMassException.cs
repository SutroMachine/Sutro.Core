using System;

namespace Sutro.Core.FunctionalTest.Exceptions
{
    public class CenterOfMassException : Exception
    {
        public CenterOfMassException(string s) : base(s)
        {
        }
    }
}