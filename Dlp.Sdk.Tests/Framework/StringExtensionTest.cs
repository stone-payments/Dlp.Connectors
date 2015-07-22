using Dlp.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Dlp.Sdk.Tests.Framework {

    [ExcludeFromCodeCoverage]
    [TestClass]
    public class StringExtensionsTest {

        [TestMethod]
        public void ConvertStringToByteArray() {

            // Teste com palavras acentuadas.
            string source = "Codificação";

            byte[] actual = source.GetBytes();

            byte[] expected = new byte[] { 67, 111, 100, 105, 102, 105, 99, 97, 195, 167, 195, 163, 111 };

            // O teste falha caso o objeto retornado seja nulo.
            Assert.IsNotNull(actual);

            // O teste falha, caso os dois objetos não possuam o mesmo tamanho.
            Assert.AreEqual(expected.Length, actual.Length);

            // O teste falha caso qualquer elemento seja diferente.
            for (int i = 0; i < actual.Length; i++) {

                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        public void ConvertStringToByteArrayWithCustomEncoding() {

            // Teste com palavras acentuadas.
            string source = "Codificação";

            byte[] actual = source.GetBytes(Encoding.GetEncoding("ISO-8859-1"));

            byte[] expected = new byte[] { 67, 111, 100, 105, 102, 105, 99, 97, 231, 227, 111 };

            // O teste falha caso o objeto retornado seja nulo.
            Assert.IsNotNull(actual);

            // O teste falha, caso os dois objetos não possuam o mesmo tamanho.
            Assert.AreEqual(expected.Length, actual.Length);

            // O teste falha caso qualquer elemento seja diferente.
            for (int i = 0; i < actual.Length; i++) {

                Assert.AreEqual(expected[i], actual[i]);
            }
        }

        [TestMethod]
        public void ConverNullStringToByteArray() {

            byte[] actual = StringExtensions.GetBytes(null);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ConvertEmptyStringToByteArray() {

            byte[] actual = StringExtensions.GetBytes(string.Empty);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ExtractDigits() {

            string source = "4111.1111.1111.1111";

            string expected = "4111111111111111";

            string actual = source.GetDigits();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ExtractDigitsFromNullString() {

            string actual = StringExtensions.GetDigits(null);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ExtractLetters() {

            string source = "RJ45, RJ12";

            string expected = "RJRJ";

            string actual = source.GetLetters();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ExtractWithAccentuatedLetters() {

            string source = "Acentuação25";

            string expected = "Acentuação";

            string actual = source.GetLetters();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ExtractLettersFromNullString() {

            string actual = StringExtensions.GetLetters(null);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ExtractLettersOrDigits() {

            string source = "RJ45, RJ12";

            string expected = "RJ45RJ12";

            string actual = source.GetLettersOrDigits();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ExtractLettersOrDigitsFromNullString() {

            string actual = StringExtensions.GetLettersOrDigits(null);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void MaskPassword() {

            string source = "Text to mask";

            string expected = "************";

            string actual = source.Mask(StringMaskFormat.Password);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MaskCreditCard() {

            string source = "4111111111111111";

            string expected = "************1111";

            string actual = source.Mask(StringMaskFormat.CreditCard);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MaskCreditCardExtended() {

            string source = "4111111111111111";

            string expected = "411111******1111";

            string actual = source.Mask(StringMaskFormat.CreditCardExtended);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MaskEmptyString() {

            string actual = string.Empty.Mask(StringMaskFormat.CreditCard);

            Assert.AreEqual(string.Empty, actual);
        }

        [TestMethod]
        public void MaskInvalidCreditCard() {

            string source = "555";

            string expected = "***";

            string actual = source.Mask(StringMaskFormat.CreditCard);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MaskInvalidCredtCardExtended() {

            string source = "411111111111";

            string expected = "********1111";

            string actual = source.Mask(StringMaskFormat.CreditCardExtended);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void MaskCreditCardWithInterrogationMark() {

            string source = "4111111111111111";

            string expected = "????????????1111";

            string actual = source.Mask(StringMaskFormat.CreditCard, '?');

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveAccentuation() {

            string source = "Acentuação de texto";

            string expected = "Acentuacao de texto";

            string actual = source.RemoveAccentuation();

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void RemoveAccentuationFromNullString() {

            string actual = StringExtensions.RemoveAccentuation(null);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ValidateInvalidCpf() {

            string source = "445445445-02";

            bool actual = source.IsValidCpf();

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void ValidateValidCpf() {

            string source = "091.853.447-02";

            bool actual = source.IsValidCpf();

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void ValidateEmptyCpf() {

            bool actual = StringExtensions.IsValidCpf(string.Empty);

            Assert.IsFalse(actual);
        }

		[TestMethod]
		public void ValidateSingleValueCpf() {

			string source = "111.111.111-11";

			bool actual = source.IsValidCpf();

			Assert.IsFalse(actual);
		}

        [TestMethod]
        public void ValidateMissingDigitsCpf() {

            bool actual = "091853".IsValidCpf();

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void ValidateInvalidCnpj() {

            string source = "45.445.445/4454-45";

            bool actual = source.IsValidCnpj();

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void ValidateValidCnpj() {

            string source = "13.525.010/0001-92";

            bool actual = source.IsValidCnpj();

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void ValidateEmptyCnpj() {

            bool actual = StringExtensions.IsValidCnpj(string.Empty);

            Assert.IsFalse(actual);
        }

		[TestMethod]
		public void ValidateSingleValueCnpj() {

			string source = "22.222.222/2222-22";

			bool actual = source.IsValidCnpj();

			Assert.IsFalse(actual);
		}

        [TestMethod]
        public void ValidateMissingDigitsCnpj() {

            bool actual = "091853".IsValidCnpj();

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void ValidateValidEmail() {

            bool actual = "sample@email.com".IsValidEmailAddress();

            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void ValidateIncompleteEmail() {

            bool actual = "sample@".IsValidEmailAddress();

            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void ValidateNullEmail() {

            bool actual = StringExtensions.IsValidEmailAddress(null);

            Assert.IsFalse(actual);
        }

		[TestMethod]
		public void CalculateMd5() {

			string source = "banana_1234";
			string actual = source.CalculateMd5();

			Assert.AreEqual("28D691496C6FF1C0C2B68C45AD171130", actual);
		}

        [TestMethod]
        public void CalculateMd5WithSecretKey() {

            string secretKey = "CalculateMd5";

            string source = "banana_1234";
            string actual = source.CalculateMd5(secretKey);

            Assert.AreEqual("33960F75A5E8809E2386C3203E5B0D85", actual);
        }

        [TestMethod]
        public void CalculateMd5NullSourceString() {

            string secretKey = "CalculateNullMd5";

            string actual = StringExtensions.CalculateMd5(null, secretKey);

            Assert.IsNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CalculateMd5NullSecretKey() {

            string source = "banana_1234";
            string actual = source.CalculateMd5(null);
        }

        [TestMethod]
        public void CalculateSha1() {

            string secretKey = "CalculateSha1";
            string source = "banana_1234";

            string actual = source.CalculateSha1(secretKey);

            Assert.AreEqual("AFF51C213B7BA299CAE9CAFD9A243A569A6FE83E", actual);
        }

        [TestMethod]
        public void CalculateSha1NullSourceString() {

            string secretKey = "CalculateSha1";

            string actual = StringExtensions.CalculateSha1(null, secretKey);

            Assert.IsNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CalculateSha1NullSecretKey() {

            string source = "banana_1234";
            string actual = source.CalculateSha1(null);
        }

        [TestMethod]
        public void CalculateSha256() {

            string secretKey = "CalculateSha256";
            string source = "banana_1234";

            string actual = source.CalculateSha256(secretKey);

            Assert.AreEqual("582763E742E55047899D7BE5C5605205A456A997BC1164FD66DF2BF4F97EDFC5", actual);
        }

        [TestMethod]
        public void CalculateSha256NullSourceString() {

            string secretKey = "CalculateSha256";

            string actual = StringExtensions.CalculateSha256(null, secretKey);

            Assert.IsNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CalculateSha256NullSecretKey() {

            string source = "banana_1234";
            string actual = source.CalculateSha256(null);
        }

        [TestMethod]
        public void CalculateSha384() {

            string secretKey = "CalculateSha384";
            string source = "banana_1234";

            string actual = source.CalculateSha384(secretKey);

            Assert.AreEqual("49A370C2163EEA87C1DA7417CDCE21EAB64E94C38F5CA3206AB85348181AA70C1ACC62D547254AC35F10013EE7517C05", actual);
        }

        [TestMethod]
        public void CalculateSha384NullSourceString() {

            string secretKey = "CalculateSha384";

            string actual = StringExtensions.CalculateSha384(null, secretKey);

            Assert.IsNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CalculateSha384NullSecretKey() {

            string source = "banana_1234";
            string actual = source.CalculateSha384(null);
        }

        [TestMethod]
        public void CalculateSha512() {

            string secretKey = "CalculateSha512";
            string source = "banana_1234";

            //string secretKey = "DC0F540CA97E4A577FF4F363D4BADBAC16A2E64E";
            //string source = "76.382.724/0001-09-LÊ TESTÉRERSçâéîã";

            string actual = source.CalculateSha512(secretKey);

            Assert.AreEqual("058BD4D552998DC743AA59ACD76344EAA0F760FF5618BDC07D6D442A111B60858E02ACA1E96A33073F0B8C8A38CFEE087FC503440A22CBC190A0617A50CFD7E9", actual);
        }

        [TestMethod]
        public void CalculateSha512NullSourceString() {

            string secretKey = "CalculateSha512";

            string actual = StringExtensions.CalculateSha512(null, secretKey);

            Assert.IsNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void CalculateSha512NullSecretKey() {

            string source = "banana_1234";
            string actual = source.CalculateSha512(null);
        }

        [TestMethod]
        public void Encrypt() {

            string secretKey = "SecretKey";
            string source = "banana_1234";
            string expected = "166F82B46512354420F4B3800FB79F81";

            string actual = source.Encrypt(secretKey);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void EncryptNullSourceString() {

            string secretKey = "SecretKey";

            string actual = StringExtensions.Encrypt(null, secretKey);

            Assert.IsNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EncryptNullSecretKey() {

            string source = "banana_1234";
            string actual = source.Encrypt(null);
        }

        [TestMethod]
        public void Decrypt() {

            string secretKey = "SecretKey";
            string source = "166F82B46512354420F4B3800FB79F81";
            string expected = "banana_1234";

            string actual = source.Decrypt(secretKey);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void DecryptNullSourceString() {

            string secretKey = "SecretKey";

            string actual = StringExtensions.Decrypt(null, secretKey);

            Assert.IsNull(actual);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void DecryptNullSecretKey() {

            string source = "banana_1234";
            string actual = source.Decrypt(null);
        }
    }
}
