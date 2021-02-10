using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Sutro.Core.Settings.Machine;

namespace Sutro.Core.UnitTests
{
    [TestClass]
    public class AdditiveSettingsTests
    {
        [TestMethod]
        public void Clone()
        {
            // Arrange
            var settings = MachineProfilesFactoryFFF.Prusa.Create_i3Mk3();

            // Act
            var clone = settings.Clone();

            // Asert
            Assert.IsNotNull(clone);
            Assert.IsInstanceOfType(clone, typeof(MachineProfileFFF));
        }

        [TestMethod]
        public void Serialize()
        {
            // Arrange
            var settings = MachineProfilesFactoryFFF.Prusa.Create_i3Mk3();

            // Act
            var json = JsonConvert.SerializeObject(settings);
        }
    }
}