using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.Settings;
using System.Collections.Generic;

namespace gsCore.UnitTests
{
    internal class SubSettingsGood
    {
        public int SubFieldX = 0;
        public int SubFieldY = 0;
    }

    internal class SubSettingsBad
    {
        public int SubFieldX = 0;
        public int SubFieldY = 0;
    }

    internal class SettingsA
    {
        public int IntegerFieldA = 0;
        public int IntegerPropertyA { get; set; } = 0;
        public string StringFieldA = "";
    }

    internal class SettingsB : SettingsA
    {
        public int IntegerFieldB = 0;
    }

    internal class SettingsC : SettingsA
    {
        public int IntegerFieldC = 0;
    }

    internal class SettingsD
    {
        public SubSettingsGood SubSettings = new SubSettingsGood();
    }

    internal class SettingsE
    {
        public SubSettingsBad SubSettings = new SubSettingsBad();
    }

    internal class SettingsWithListOfDouble
    {
        public List<double> ListOfDouble = new List<double>() { 0, 1 };
    }

    internal class SettingsWithListOfSubsetting
    {
        public List<SubSettingsGood> ListOfSubsetting = new List<SubSettingsGood>()
        { new SubSettingsGood() };
    }

    internal enum EnumNumbers { Zero = 0, One = 1, Two = 2 };

    internal class SettingsF
    {
        public EnumNumbers Enum;
    }

    internal enum EnumColors { Blue = 0, Green = 1, Red = 2 };

    internal class SettingsG
    {
        public EnumColors Enum;
    }

    internal class SettingsH
    {
        public float[,] FloatArray = new float[2, 3] { { 0, 1, 2 }, { 3, 4, 5 } };
    }

    [TestClass]
    public class SettingsTests
    {
        [TestMethod]
        public void CopyFromSameType_Field_ValueType()
        {
            var orig = new SettingsA();
            var copy = new SettingsA();
            orig.IntegerFieldA = 3;

            SettingsPrototype.CopyValuesFrom(copy, orig);

            Assert.AreEqual(3, copy.IntegerFieldA);
        }

        [TestMethod]
        public void CopyFromSameType_Property_ValueType()
        {
            var orig = new SettingsA();
            var copy = new SettingsA();
            orig.IntegerPropertyA = 5;

            SettingsPrototype.CopyValuesFrom(copy, orig);

            Assert.AreEqual(5, copy.IntegerPropertyA);
        }

        [TestMethod]
        public void CopyFromChild_Field_ValueType()
        {
            var orig = new SettingsB();
            var copy = new SettingsA();
            orig.IntegerFieldA = 7;

            SettingsPrototype.CopyValuesFrom(copy, orig);

            Assert.AreEqual(7, copy.IntegerFieldA);
        }

        [TestMethod]
        public void CopyFromParent_Field_ValueType()
        {
            var orig = new SettingsA();
            var copy = new SettingsB();
            orig.IntegerFieldA = 8;

            SettingsPrototype.CopyValuesFrom(copy, orig);

            Assert.AreEqual(8, copy.IntegerFieldA);
        }

        [TestMethod]
        public void CopyFromSibling_Field_ValueType()
        {
            var orig = new SettingsB();
            var copy = new SettingsC();
            orig.IntegerFieldA = 9;

            SettingsPrototype.CopyValuesFrom(copy, orig);

            Assert.AreEqual(9, copy.IntegerFieldA);
        }

        [TestMethod]
        public void CloneFromParent()
        {
            var orig = new SettingsA();
            orig.IntegerFieldA = 4;

            var copy = SettingsPrototype.CloneAs<SettingsB, SettingsA>(orig);

            Assert.IsNotNull(copy);
            Assert.AreEqual(4, copy.IntegerFieldA);
        }

        [TestMethod]
        public void CloneFromChild()
        {
            var orig = new SettingsB();
            orig.IntegerFieldA = 4;

            var copy = SettingsPrototype.CloneAs<SettingsA, SettingsB>(orig);

            Assert.IsNotNull(copy);
            Assert.AreEqual(4, copy.IntegerFieldA);
        }

        [TestMethod]
        public void Clone_StringsIndependant()
        {
            var orig = new SettingsA();
            orig.StringFieldA = "hello";

            var copy = SettingsPrototype.CloneAs<SettingsA, SettingsA>(orig);
            orig.StringFieldA = "world";

            Assert.AreEqual("hello", copy.StringFieldA);
        }

        [TestMethod]
        public void Clone_ReferenceTypesIndependant()
        {
            var orig = new SettingsD();
            orig.SubSettings.SubFieldX = 1;

            var copy = SettingsPrototype.CloneAs<SettingsD, SettingsD>(orig);
            orig.SubSettings.SubFieldX = 2;

            Assert.AreEqual(1, copy.SubSettings.SubFieldX);
        }

        [TestMethod]
        public void Clone_FieldsWithDifferentEnums()
        {
            var orig = new SettingsF();
            orig.Enum = EnumNumbers.One;

            var copy = new SettingsG();
            copy.Enum = EnumColors.Red;

            SettingsPrototype.CopyValuesFrom(copy, orig);

            Assert.AreEqual(EnumColors.Red, copy.Enum);
        }

        [TestMethod]
        public void Clone_FieldsWithSameEnums()
        {
            var orig = new SettingsF();
            orig.Enum = EnumNumbers.One;

            var copy = new SettingsF();
            copy.Enum = EnumNumbers.Two;

            SettingsPrototype.CopyValuesFrom(copy, orig);

            Assert.AreEqual(EnumNumbers.One, copy.Enum);
        }

        [TestMethod]
        public void Clone_FloatArray()
        {
            var orig = new SettingsH();
            orig.FloatArray[0, 0] = 10;

            var copy = SettingsPrototype.CloneAs<SettingsH, SettingsH>(orig);
            orig.FloatArray[0, 0] = 99;

            Assert.AreEqual(10, copy.FloatArray[0, 0]);
        }

        [TestMethod]
        public void Clone_DoubleList()
        {
            var orig = new SettingsWithListOfDouble();
            orig.ListOfDouble.Add(9);

            var copy = SettingsPrototype.CloneAs<
                SettingsWithListOfDouble, SettingsWithListOfDouble>(orig);
            orig.ListOfDouble[0] = 7;
            orig.ListOfDouble[1] = 8;

            Assert.AreEqual(3, copy.ListOfDouble.Count);
            Assert.AreEqual(0, copy.ListOfDouble[0]);
            Assert.AreEqual(1, copy.ListOfDouble[1]);
            Assert.AreEqual(9, copy.ListOfDouble[2]);
        }

        [TestMethod]
        public void Clone_SubsettingList()
        {
            var orig = new SettingsWithListOfSubsetting();
            orig.ListOfSubsetting[0].SubFieldX = 0;
            orig.ListOfSubsetting[0].SubFieldY = 1;

            var copy = SettingsPrototype.CloneAs<
                SettingsWithListOfSubsetting, SettingsWithListOfSubsetting>(orig);

            orig.ListOfSubsetting[0].SubFieldX = 10;
            orig.ListOfSubsetting[0].SubFieldY = 20;
            orig.ListOfSubsetting.Add(new SubSettingsGood());

            Assert.AreEqual(1, copy.ListOfSubsetting.Count);
            Assert.AreEqual(0, copy.ListOfSubsetting[0].SubFieldX);
            Assert.AreEqual(1, copy.ListOfSubsetting[0].SubFieldY);
        }
    }
}