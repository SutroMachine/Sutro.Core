using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sutro.Core.Parallel
{
    public interface IParallelActor
    {
        void ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions options, Action<TSource> body);
    }
}