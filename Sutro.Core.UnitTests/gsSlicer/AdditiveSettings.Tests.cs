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
            orig.Part.Shells = 10;
            orig.Machine.NozzleDiamMM = 20;
            orig.Machine.ManufacturerName = "A";

            // act
            var copy = SettingsPrototype.CloneAs<FlashforgeSettings, FlashforgeSettings>(orig);

            // assert
            Assert.AreEqual(10, copy.Part.Shells);
            Assert.AreEqual(20, copy.Machine.NozzleDiamMM);
            Assert.AreEqual("A", orig.Machine.ManufacturerName);
            Assert.AreNotSame(copy.Machine, orig.Machine);
        }

        [TestMethod]
        public void CloneAs_CloneValuesDoNotAffectOriginal()
        {
            // arrange
            var orig = new GenericRepRapSettings();
            orig.Part.Shells = 10;
            orig.Machine.NozzleDiamMM = 20;
            orig.Machine.ManufacturerName = "A";

            // act
            GenericRepRapSettings copy = SettingsPrototype.CloneAs<GenericRepRapSettings, GenericRepRapSettings>(orig);
            copy.Part.Shells *= 2;
            copy.Machine.NozzleDiamMM *= 20;
            copy.Machine.ManufacturerName = "B";

            // assert
            Assert.AreEqual(10, orig.Part.Shells);
            Assert.AreEqual(20, orig.Machine.NozzleDiamMM);
            Assert.AreEqual("A", orig.Machine.ManufacturerName);
            Assert.AreNotSame(copy.Machine, orig.Machine);
        }

        [TestMethod]
        public void CloneAs_ToDerivedClass()
        {
            // arrange
            var orig = new GenericPrinterSettings("", "", "");

            // act
            var clone = SettingsPrototype.CloneAs<GenericRepRapSettings, GenericPrinterSettings>(orig);

            // assert
            Assert.IsNotNull(clone);
        }

        [TestMethod]
        public void CloneAs_ToParentClass()
        {
            // arrange
            var orig = new GenericRepRapSettings();

            // act
            var clone = SettingsPrototype.CloneAs<GenericPrinterSettings, GenericRepRapSettings>(orig);

            // assert
            Assert.IsNotNull(clone);
        }

        [TestMethod]
        public void CloneAs_SiblingClass()
        {
            // arrange
            var orig = PrusaSettings.Create_i3MK3();

            // act
            var clone = SettingsPrototype.CloneAs<FlashforgeSettings, PrusaSettings>(orig);

            // assert
            Assert.IsNotNull(clone);
        }
    }
}