using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sutro.Core.Logging;
using Sutro.Core.Settings.Part;
using Sutro.Core.Utility;
using System.Linq;

namespace Sutro.Core.UnitTests
{
    public class MockNestedClass
    {
        public int PropertyA { get; set; } = 0;
        public int PropertyB { get; set; } = 0;
    }

    public class MockPartProfile : PartProfileFFF
    {
        public MockNestedClass NestedClass { get; set; } = new MockNestedClass();
    }

    [TestClass()]
    public class SettingsBuilderTests
    {
        [TestMethod()]
        public void ApplyJsonSnippet_BasicProperty()
        {
            // Arrange
            var settingsBuilder = CreateSettingsBuilder();

            // Act
            settingsBuilder.ApplyJSONSnippet("LayerHeightMM:0.345");

            // Assert
            Assert.AreEqual(0.345, settingsBuilder.Settings.LayerHeightMM, 1e-6);
        }

        [TestMethod()]
        public void ApplyJsonSnippet_ListOfDoubles()
        {
            // Arrange
            var settingsBuilder = CreateSettingsBuilder();

            // Act
            settingsBuilder.ApplyJSONSnippet("InfillAngles:[0,30,60]");

            // Assert
            Assert.AreEqual(3, settingsBuilder.Settings.InfillAngles.Count);
        }

        [TestMethod()]
        public void ApplyJsonSnippet_NestedClass()
        {
            // Arrange
            var settings = new MockPartProfile();
            var settingsBuilder = new SettingsBuilder<MockPartProfile>(settings, new NullLogger());
            settings.NestedClass.PropertyA = 3;
            settings.NestedClass.PropertyB = 4;

            // Act
            settingsBuilder.ApplyJSONSnippet("NestedClass.PropertyB:5");

            // Assert
            Assert.AreEqual(3, settingsBuilder.Settings.NestedClass.PropertyA);
            Assert.AreEqual(5, settingsBuilder.Settings.NestedClass.PropertyB);
        }

        [TestMethod()]
        public void ApplyJsonSnippet_Interval1i()
        {
            // Arrange
            var settingsBuilder = CreateSettingsBuilder();
            settingsBuilder.Settings.LayerRangeFilter = new g3.Interval1i(9, 99);

            // Act
            settingsBuilder.ApplyJSONSnippet("LayerRangeFilter.Max:999");

            // Assert
            Assert.AreEqual(9, settingsBuilder.Settings.LayerRangeFilter.a);
            Assert.AreEqual(999, settingsBuilder.Settings.LayerRangeFilter.b);
        }

        private static SettingsBuilder<PartProfileFFF> CreateSettingsBuilder()
        {
            var settings = PartProfileFactoryFFF.EnumerateDefaults().First();
            var settingsBuilder = new SettingsBuilder<PartProfileFFF>(settings, new NullLogger());
            return settingsBuilder;
        }
    }
}