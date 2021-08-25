using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.Parallel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Sutro.Core.UnitTests.Parallel
{
    [TestClass]
    public class ParallelActorTests
    {
        [TestMethod]
        public void ParallelActorAll()
        {
            // Arrange
            var actor = new ParallelActorAll();
            var threads = new ConcurrentDictionary<int, int>();

            // Act
            actor.ForEach(Enumerable.Range(0, 10), new ParallelOptions(), (int i) =>
            {
                int id = Environment.CurrentManagedThreadId;
                threads.TryAdd(id, 0);
                threads[id] += 1;
                Thread.Sleep(200);
            });

            // Assert
            Assert.AreNotEqual(1, threads.Count);
        }

        [TestMethod]
        public void ParallelActorNone()
        {
            // Arrange
            var actor = new ParallelActorNone();
            var threads = new ConcurrentDictionary<int, int>();

            // Act
            actor.ForEach(Enumerable.Range(0, 10), new ParallelOptions(), (int i) =>
            {
                int id = Environment.CurrentManagedThreadId;
                threads.TryAdd(id, 0);
                threads[id] += 1;
                Thread.Sleep(200);
            });

            // Assert
            Assert.AreEqual(1, threads.Count);
        }

        [TestMethod]
        public void ParallelActorIfDebuggerNotAttached()
        {
            // Arrange
            var actor = new ParallelActorIfDebuggerNotAttached();
            var threads = new ConcurrentDictionary<int, int>();

            // Act
            actor.ForEach(Enumerable.Range(0, 10), new ParallelOptions(), (int i) =>
            {
                int id = Environment.CurrentManagedThreadId;
                threads.TryAdd(id, 0);
                threads[id] += 1;
                Thread.Sleep(200);
            });

            // Assert
            if (System.Diagnostics.Debugger.IsAttached)
                Assert.AreEqual(1, threads.Count);
            else
                Assert.AreNotEqual(1, threads.Count);
        }
    }
}