using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.Settings;
using Sutro.Core.Settings.Machine;

namespace gsCore.UnitTests
{
    [TestClass]
    public class AdditiveSettingsTests
    {
        [TestMethod]
        public void CloneAs_ValuesCloneCorrectly()
        {
            // arrange
            var orig = new PrintProfileFFF();
            orig.Part.Shells = 10;
            orig.Machine.NozzleDiamMM = 20;
            orig.Machine.ManufacturerName = "A";

            // act
            var copy = SettingsPrototype.CloneAs<PrintProfileFFF, PrintProfileFFF>(orig);

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
            var orig = new PrintProfileFFF();
            orig.Part.Shells = 10;
            orig.Machine.NozzleDiamMM = 20;
            orig.Machine.ManufacturerName = "A";

            // act
            PrintProfileFFF copy = SettingsPrototype.CloneAs<PrintProfileFFF, PrintProfileFFF>(orig);
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
        public void CloneAs_SiblingClass()
        {
            // arrange
            var orig = MachineProfilesFactoryFFF.Prusa.Create_i3Mk3();

            // act
            var clone = SettingsPrototype.CloneAs<MachineProfileFFF, MachineProfileFFF>(orig);

            // assert
            Assert.IsNotNull(clone);
        }
    }
}