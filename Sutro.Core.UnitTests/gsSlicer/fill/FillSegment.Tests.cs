using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace gs.UnitTests.Fill
{
    [TestClass]
    public class FillSegmentTests
    {
        [TestMethod]
        public void Equals_True()
        {
            var seg1 = new FillSegment(true);
            var seg2 = new FillSegment(true);

            Assert.IsTrue(seg1.Equals(seg2));
        }

        [TestMethod]
        public void Equals_False()
        {
            var seg1 = new FillSegment(true);
            var seg2 = new FillSegment(false);

            Assert.IsFalse(seg1.Equals(seg2));
        }

        [TestMethod]
        public void Equals_False_DifferentTYpe()
        {
            var seg1 = new FillSegment(true);

            Assert.IsFalse(seg1.Equals(3));
        }
    }
}