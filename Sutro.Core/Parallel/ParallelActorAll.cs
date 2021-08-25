using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sutro.Core.Parallel
{
    public class ParallelActorAll : IParallelActor
    {
        public void ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions options, Action<TSource> body)
        {
            System.Threading.Tasks.Parallel.ForEach(source, options, body);
        }
    }
}