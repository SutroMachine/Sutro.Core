using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace gs.UnitTests.Fill
{
    [TestClass]
    public class ElementLocationTests
    {
        [TestMethod]
        public void ParameterizedDistance_Set_Valid()
        {
            var location = new ElementLocation(0, 0);
            location.ParameterizedDistance = 0;
            location.ParameterizedDistance = 0.5;
            location.ParameterizedDistance = 1;
        }

        [TestMethod]
        public void ParameterizedDistance_Set_Invalid_LessThanZero()
        {
            var location = new ElementLocation(0, 0);
            Assert.ThrowsException<ArgumentException>(() =>
            {
                location.ParameterizedDistance = -1;
            });
        }

        [TestMethod]
        public void ParameterizedDistance_Set_Invalid_MoreThanOne()
        {
            var location = new ElementLocation(0, 0);
            Assert.ThrowsException<ArgumentException>(() =>
            {
                location.ParameterizedDistance = 2;
            });
        }
    }
}