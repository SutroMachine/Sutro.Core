using Sutro.Core.Models;
using System;
using System.Collections.Generic;

namespace Sutro.Core.Utility
{
    public static class Parallel
    {
        public static void ForEach<T>(IEnumerable<T> source, Action<T, long> action)
        {
            if (Config.Parallel)
            {
                System.Threading.Tasks.Parallel.ForEach(source, (body, state, index) => action(body, index));
            }
            else
            {
                int i = 0;
                foreach (var o in source)
                {
                    action?.Invoke(o, i++);
                }
            }
        }
    }
}