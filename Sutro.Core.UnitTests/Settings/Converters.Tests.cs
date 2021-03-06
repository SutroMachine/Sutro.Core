﻿using g3;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Sutro.Core.Settings;

namespace Sutro.Core.UnitTests.Settings
{
    [TestClass()]
    public class ConvertersTests
    {
        [TestMethod()]
        public void Interval1iRoundTrip()
        {
            var interval = new Interval1i(3, 6);

            var converter = new Interval1iConverter();
            var serializerSettings = new JsonSerializerSettings() { Converters = new JsonConverter[] { converter } };

            var json = JsonConvert.SerializeObject(interval, serializerSettings);

            var result = JsonConvert.DeserializeObject<Interval1i>(json, serializerSettings);

            Assert.AreEqual(interval.a, result.a);
            Assert.AreEqual(interval.b, result.b);
        }
    }
}