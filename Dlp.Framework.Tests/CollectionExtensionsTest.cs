using Dlp.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace Dlp.Sdk.Tests {

    [ExcludeFromCodeCoverage]
    [TestClass]
    public class CollectionExtensionsTest {

        [TestMethod]
        public void ConvertListOfIntToString() {

            List<int> source = new List<int>();

            for (int i = 0; i < 10; i++) { source.Add(i); }

            string expected = "0,1,2,3,4,5,6,7,8,9";

            string actual = source.AsString();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ConvertIEnumerableOfGuidToSurroundedString() {

            Guid a = Guid.NewGuid();
            Guid b = Guid.NewGuid();
            Guid c = Guid.NewGuid();

            IEnumerable<Guid> source = new List<Guid>(new Guid[] { a, b, c });

            string expected = string.Format("'{0}','{1}','{2}'", a, b, c);

            string actual = source.AsString(',', "'");

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ConvertCollectionOfStringToCollonSeparatedString() {

            Collection<string> source = new Collection<string>();

            source.Add("One");
            source.Add("Two");
            source.Add("Three");
            source.Add("Four");
            source.Add("Five");

            string expected = "One:Two:Three:Four:Five";

            string actual = source.AsString(':');

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ConvertNullCollectionToString() {

            string actual = CollectionExtensions.AsString(null);

            Assert.IsNull(actual);
        }
    }
}
