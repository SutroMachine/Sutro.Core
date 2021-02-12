using System;

namespace Sutro.Core.FunctionalTest
{
    public class MissingFeatureException : Exception
    {
        public MissingFeatureException(string s) : base(s)
        {
        }
    }
}