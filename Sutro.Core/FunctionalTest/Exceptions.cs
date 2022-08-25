using System;

namespace Sutro.Core.FunctionalTest.FeatureMismatchExceptions
{
    public class PrintsNotEquivalentException : Exception
    {
        public PrintsNotEquivalentException(string errorMessage) : base(errorMessage)
        {
        }
    }
}