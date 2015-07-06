using Dlp.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Dlp.Sdk.Tests.Framework {

    [ExcludeFromCodeCoverage]
    [TestClass]
    public class DateTimeExtensionsTest {

        [TestMethod]
        public void ChangeUtcToBrasiliaTimeZone() {

            DateTime source = DateTime.UtcNow;

            DateTime expected = DateTime.Now;

            DateTime actual = source.ChangeTimeZone(TimeZoneInfo.Utc.Id, "E. South America Standard Time");
        }

        [TestMethod]
        public void ChangeBrasiliaToHawaiTimeZone() {

            DateTime source = DateTime.Now;

            DateTime expected = source.AddHours(-7);

            DateTime actual = source.ChangeTimeZone("E. South America Standard Time", "Hawaiian Standard Time");
        }

        [TestMethod]
        public void ConvertToIso8061String() {

            DateTime source = new DateTime(2010, 09, 01, 10, 30, 50);

            string expected = "2010-09-01T10:30:50";

            string actual = source.ToIso8601String();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void SystemTimeZonesTest() {

            IDictionary<string, string> result = DateTimeExtensions.SystemTimeZones();

            Assert.IsNotNull(result);

            Assert.IsTrue(result.Any());
        }
    }
}
