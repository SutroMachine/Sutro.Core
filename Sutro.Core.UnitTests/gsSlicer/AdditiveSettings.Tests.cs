using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.Settings;
using Sutro.Core.Settings.Info;

namespace gsCore.UnitTests
{
    [TestClass]
    public class AdditiveSettingsTests
    {
        [TestMethod]
        public void CloneAs_ValuesCloneCorrectly()
        {
            // arrange
            var orig = new FlashforgeSettings();
            orig.PartProfile.Shells = 10;
            orig.MachineProfile.NozzleDiamMM = 20;
            orig.MachineProfile.ManufacturerName = "A";

            // act
            var copy = orig.CloneAs<FlashforgeSettings>();

            // assert
            Assert.AreEqual(10, copy.PartProfile.Shells);
            Assert.AreEqual(20, copy.MachineProfile.NozzleDiamMM);
            Assert.AreEqual("A", orig.MachineProfile.ManufacturerName);
            Assert.AreNotSame(copy.MachineProfile, orig.MachineProfile);
        }

        [TestMethod]
        public void CloneAs_CloneValuesDoNotAffectOriginal()
        {
            // arrange
            var orig = new GenericRepRapSettings();
            orig.PartProfile.Shells = 10;
            orig.MachineProfile.NozzleDiamMM = 20;
            orig.MachineProfile.ManufacturerName = "A";

            // act
            GenericRepRapSettings copy = orig.CloneAs<GenericRepRapSettings>();
            copy.PartProfile.Shells *= 2;
            copy.MachineProfile.NozzleDiamMM *= 20;
            copy.MachineProfile.ManufacturerName = "B";

            // assert
            Assert.AreEqual(10, orig.PartProfile.Shells);
            Assert.AreEqual(20, orig.MachineProfile.NozzleDiamMM);
            Assert.AreEqual("A", orig.MachineProfile.ManufacturerName);
            Assert.AreNotSame(copy.MachineProfile, orig.MachineProfile);
        }

        [TestMethod]
        public void CloneAs_ToDerivedClass()
        {
            // arrange
            var orig = new GenericPrinterSettings("", "", "");

            // act
            var clone = orig.CloneAs<GenericRepRapSettings>();

            // assert
            Assert.IsNotNull(clone);
        }

        [TestMethod]
        public void CloneAs_ToParentClass()
        {
            // arrange
            var orig = new GenericRepRapSettings();

            // act
            var clone = orig.CloneAs<GenericPrinterSettings>();

            // assert
            Assert.IsNotNull(clone);
        }

        [TestMethod]
        public void CloneAs_SiblingClass()
        {
            // arrange
            var orig = PrusaSettings.Create_i3MK3();

            // act
            var clone = orig.CloneAs<FlashforgeSettings>();

            // assert
            Assert.IsNotNull(clone);
        }
    }
}