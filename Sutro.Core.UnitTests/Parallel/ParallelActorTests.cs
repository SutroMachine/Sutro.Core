using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.Parallel;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var result = new List<int>();
            var notExpected = Enumerable.Range(0, 10).ToList();

            // Act
            actor.ForEach(Enumerable.Range(0, 10), new ParallelOptions(), (int i) => result.Add(i));

            // Assert
            CollectionAssert.AreNotEqual(notExpected, result);

        }

        [TestMethod]
        public void ParallelActorNone()
        {
            // Arrange
            var actor = new ParallelActorNone();
            var result = new List<int>();
            var expected = Enumerable.Range(0, 10).ToList();

            // Act
            actor.ForEach(Enumerable.Range(0, 10), new ParallelOptions(), (int i) => result.Add(i));

            // Assert
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ParallelActorIfDebuggerNotAttached()
        {
            // Arrange
            var actor = new ParallelActorIfDebuggerNotAttached();
            var result = new List<int>();
            var expected = Enumerable.Range(0, 10).ToList();

            // Act
            actor.ForEach(Enumerable.Range(0, 10), new ParallelOptions(), (int i) => result.Add(i));
        }
    }
}