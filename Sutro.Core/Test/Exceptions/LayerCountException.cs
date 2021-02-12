using System;

namespace Sutro.Core.Test.Exceptions
{
    public class LayerCountException : Exception
    {
        public LayerCountException(string s) : base(s)
        {
        }
    }
}