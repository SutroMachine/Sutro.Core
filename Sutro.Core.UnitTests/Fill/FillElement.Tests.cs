using g3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.Fill;

namespace Sutro.Core.UnitTests.Fill
{
    [TestClass]
    public class FillElementTests
    {
        private const double tolerance = 1e-6;

        private FillElement<FillSegment> ConstructElement()
        {
            return new FillElement<FillSegment>(new Vector2d(0, 0), new Vector2d(1, 0), new FillSegment(false));
        }

        [TestMethod]
        public void GetSegment2d()
        {
            var element = ConstructElement();
            var seg = element.GetSegment2d();
            Assert.AreEqual(0, seg.P0.x, tolerance);
            Assert.AreEqual(0, seg.P0.y, tolerance);
            Assert.AreEqual(1, seg.P1.x, tolerance);
            Assert.AreEqual(0, seg.P1.y, tolerance);
        }

        [TestMethod]
        public void GetHashCodes_Equal()
        {
            var element1 = ConstructElement();
            var element2 = ConstructElement();

            var hashCode1 = element1.GetHashCode();
            var hashCode2 = element2.GetHashCode();

            Assert.AreEqual(hashCode1, hashCode2);
        }

        [TestMethod]
        public void Equals_True()
        {
            var element1 = ConstructElement();
            var element2 = ConstructElement();

            Assert.IsTrue(element1.Equals(element2));
        }

        [TestMethod]
        public void Equals_False_DifferentNode()
        {
            var element1 = ConstructElement();
            var element2 = new FillElement<FillSegment>(new Vector2d(0, 0), new Vector2d(2, 0), new FillSegment(false));

            Assert.IsFalse(element1.Equals(element2));
        }

        [TestMethod]
        public void Equals_False_DifferentSegment()
        {
            var element1 = ConstructElement();
            var element2 = new FillElement<FillSegment>(new Vector2d(0, 0), new Vector2d(2, 0), new FillSegment(true));

            Assert.IsFalse(element1.Equals(element2));
        }

        [TestMethod]
        public void Equals_False_DifferentType()
        {
            var element1 = ConstructElement();
            double other = 1;

            Assert.IsFalse(element1.Equals(other));
        }
    }
}