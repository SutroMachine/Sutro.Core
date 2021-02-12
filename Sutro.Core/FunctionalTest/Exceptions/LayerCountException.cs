using System;

namespace Sutro.Core.FunctionalTest.Exceptions
{
    public class LayerCountException : Exception
    {
        public LayerCountException(string s) : base(s)
        {
        }
    }
}