using g3;
using gs.info;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Security;

namespace Sutro.Core.UnitTests
{
    [TestClass]
    public class AdditiveSettingsTests
    {
        [TestMethod]
        public void Clone()
        {
            // Arrange
            var settings = new PrusaSettings();

            // Act
            var clone = settings.Clone();

            // Asert
            Assert.IsNotNull(clone);
            Assert.IsInstanceOfType(clone, typeof(PrusaSettings));
        }

        [TestMethod]
        public void Serialize()
        {
            // Arrange
            var settings = new PrusaSettings();

            // Act
            var json = JsonConvert.SerializeObject(settings);

        }
    }
}
