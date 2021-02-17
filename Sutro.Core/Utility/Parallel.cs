using Sutro.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Sutro.Core.Utility
{
    public static class Parallel
    {
        public static void ForEach<T>(IEnumerable<T> source, CancellationToken cancellationToken, Action<T, long> action)
        {
            if (Config.Parallel)
            {
                var options = new ParallelOptions();
                options.CancellationToken = cancellationToken;
                options.MaxDegreeOfParallelism = Environment.ProcessorCount;

                System.Threading.Tasks.Parallel.ForEach(source, options, (body, state, index) =>
                {
                    options.CancellationToken.ThrowIfCancellationRequested();
                    action(body, index);
                });
            }
            else
            {
                int i = 0;
                foreach (var o in source)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    action?.Invoke(o, i++);
                }
            }
        }
    }
}