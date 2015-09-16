using Dlp.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Dlp.Sdk.Tests {

    [ExcludeFromCodeCoverage]
    [TestClass]
    public class ByteExtensionsTest {

        [TestMethod]
        public void ConvertByteArrayToString() {

            // Array de bytes gerado por encoding UTF8.
            byte[] source = new byte[] { 67, 111, 100, 105, 102, 105, 99, 97, 195, 167, 195, 163, 111 };

            string actual = source.GetString();

            // Teste com palavras ascentuadas.
            string expected = "Codificação";

            // O teste falha caso o objeto retornado seja nulo.
            Assert.IsNotNull(actual);

            // O teste falha, caso os dois objetos não sejam iguais.
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ConvertCustomEncodingByteArrayToString() {

            byte[] source = new byte[] { 67, 111, 100, 105, 102, 105, 99, 97, 231, 227, 111 };

            string actual = source.GetString(Encoding.GetEncoding("ISO-8859-1"));

            // Teste com palavras ascentuadas.
            string expected = "Codificação";

            // O teste falha caso o objeto retornado seja nulo.
            Assert.IsNotNull(actual);

            // O teste falha, caso os dois objetos não sejam iguais.
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ConvertNullByteArrayToString() {

            string actual = ByteExtensions.GetString(null);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ConvertEmptyByteArrayToString() {

            string actual = ByteExtensions.GetString(new byte[] { });

            Assert.IsNull(actual);
        }
    }
}
