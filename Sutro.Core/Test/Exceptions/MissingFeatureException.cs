using System;

namespace Sutro.Core.Test.Exceptions
{
    public class MissingFeatureException : Exception
    {
        public MissingFeatureException(string s) : base(s)
        {
        }
    }
}