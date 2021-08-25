using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Sutro.Core.Parallel
{
    public class ParallelActorIfDebuggerNotAttached : IParallelActor
    {
        public void ForEach<TSource>(IEnumerable<TSource> source, ParallelOptions options, Action<TSource> body)
        {
            IParallelActor actor;

            if (System.Diagnostics.Debugger.IsAttached)
                actor = new ParallelActorNone();
            else
                actor = new ParallelActorAll();

            actor.ForEach(source, options, body);
        }
    }
}