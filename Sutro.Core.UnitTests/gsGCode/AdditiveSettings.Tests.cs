using g3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Sutro.Core.Settings;
using Sutro.Core.Settings.Info;
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
