using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sutro.Core.Parallel
{
    public class ParallelActorSerial : IParallelActor
    {
        public void ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions options, Action<TSource> body)
        {
            foreach (var item in source)
            {
                options.CancellationToken.ThrowIfCancellationRequested();
                body?.Invoke(item);
            }
        }
    }
}