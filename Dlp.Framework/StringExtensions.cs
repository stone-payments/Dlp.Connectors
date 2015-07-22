using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System;
using System.Collections.Generic;

namespace Dlp.Framework {

    /// <summary>
    /// String extension methods.
    /// </summary>
    public static class StringExtensions {

        /// <summary>
        /// Converts a string to a byte array.
        /// </summary>
        /// <param name="source">String to be converted.</param>
        /// <param name="encoding">Encoding to be used for conversion. Default value: Encoding.UTF8.</param>
        /// <returns>Retorn a byte array generated from the source string, or null, if the string does not have data.</returns>
        /// <include file='Samples/StringExtensions.xml' path='Docs/Members[@name="GetBytes"]/*'/>
        public static byte[] GetBytes(this string source, Encoding encoding = null) {

            // Sai do método, caso a string seja inválida.
            if (string.IsNullOrWhiteSpace(source) == true) { return null; }

            // Verifica se foi especificado algum encoding.
            if (encoding == null) { encoding = Encoding.UTF8; }

            // Converte a string para array de bytes.
            return encoding.GetBytes(source);
        }

        /// <summary>
        /// Get all the digits of the specified string.
        /// </summary>
        /// <param name="source">String to be checked.</param>
        /// <returns>Return a new string containing only the extracted digits.</returns>
        /// <include file='Samples/StringExtensions.xml' path='Docs/Members[@name="GetDigits"]/*'/>
        public static string GetDigits(this string source) {

            // Sai do método, caso a string seja inválida.
            if (string.IsNullOrWhiteSpace(source) == true) { return source; }

            // Remove tudo que não for dígito.
            return new string(source.Where(char.IsDigit).ToArray());
        }

        /// <summary>
        /// Get all the letters of the specified string.
        /// </summary>
        /// <param name="source">String to be checked.</param>
        /// <returns>Return a new string containing only the extracted letters.</returns>
        public static string GetLetters(this string source) {

            // Sai do método, caso a string seja inválida.
            if (string.IsNullOrWhiteSpace(source) == true) { return source; }

            // Remove tudo que não for dígito.
            return new string(source.Where(char.IsLetter).ToArray());
        }

        /// <summary>
        /// Get all the letters and digits of the specified string.
        /// </summary>
        /// <param name="source">String to be checked.</param>
        /// <returns>Return a new string containing only the extracted letters and digits.</returns>
        /// <include file='Samples/StringExtensions.xml' path='Docs/Members[@name="GetLettersOrDigits"]/*'/>
        public static string GetLettersOrDigits(this string source) {

            // Sai do método, caso a string seja inválida.
            if (string.IsNullOrWhiteSpace(source) == true) { return source; }

            // Remove tudo que não for dígito.
            return new string(source.Where(char.IsLetterOrDigit).ToArray());
        }

        /// <summary>
        /// Masks the content of the specified string.
        /// </summary>
        /// <param name="source">String to be masked.</param>
        /// <param name="stringMaskFormat">Mask format to be used.</param>
        /// <param name="maskCharacter">Mask character. Default character is asterisk '*'</param>
        /// <returns>Return a new string with masked content.</returns>
        /// <include file='Samples/StringExtensions.xml' path='Docs/Members[@name="Mask"]/*'/>
        public static string Mask(this string source, StringMaskFormat stringMaskFormat, char maskCharacter = '*') {

            // Sai do método, caso a string seja inválida.
            if (string.IsNullOrWhiteSpace(source) == true) { return source; }

            // Verifica se é possível aplicar a mascara de cartão de crédito extendida.
            if (stringMaskFormat == StringMaskFormat.CreditCardExtended && source.Length < 14) { stringMaskFormat = StringMaskFormat.CreditCard; }

            // Verifica se é possível aplicar a máscara de cartão de crédito.
            if (stringMaskFormat == StringMaskFormat.CreditCard && source.Length < 4) { stringMaskFormat = StringMaskFormat.Password; }

            string maskedString = source;

            // Verifica o tipo de mascara a ser aplicada.
            switch (stringMaskFormat) {

                case StringMaskFormat.Password:

                    // Mascara todos os caracteres da string.
                    maskedString = string.Empty.PadLeft(source.Length, maskCharacter);

                    break;

                case StringMaskFormat.CreditCard:

                    // Obtém os últimos 4 caracteres da string.
                    string visibleChars = source.Substring(source.Length - 4, 4);

                    // Cria a string mascarada.
                    maskedString = visibleChars.PadLeft(source.Length, maskCharacter);

                    break;

                case StringMaskFormat.CreditCardExtended:

                    // Obtém os primeiros 6 caracteres da string.
                    string visibleCharsStart = source.Substring(0, 6);

                    // Obtém os últimos 4 caracteres da string.
                    string visibleCharsEnd = source.Substring(source.Length - 4, 4);

                    // Cria a string mascarada.
                    maskedString = visibleCharsStart + visibleCharsEnd.PadLeft(source.Length - 6, maskCharacter);

                    break;
            }

            return maskedString;
        }

        /// <summary>
        /// Replaces all the accented characters with its unaccented version.
        /// </summary>
        /// <param name="source">String to be checked.</param>
        /// <returns>Return a new string without accented characters.</returns>
        /// <include file='Samples/StringExtensions.xml' path='Docs/Members[@name="RemoveAccentuation"]/*'/>
        public static string RemoveAccentuation(this string source) {

            // Sai do método, caso a string seja inválida.
            if (string.IsNullOrWhiteSpace(source) == true) { return source; }

            // A codificação ISO-8859-8 (Hebrew) não armazena as informações completas de acentuação. Basta converter para array de bytes e converter novamente para string.
            Encoding encoding = Encoding.GetEncoding("ISO-8859-8");

            // Converte a string para um array de bytes.
            byte[] bytes = encoding.GetBytes(source);

            // Obtém a string a partir do array de bytes, que não possui informações de acentuação.
            return encoding.GetString(bytes);
        }

        /// <summary>
        /// Checks if the current string is a valid CPF.
        /// </summary>
        /// <param name="source">String to be checked.</param>
        /// <returns>Return true if the string is a valid CPF.</returns>
        /// <include file='Samples/StringExtensions.xml' path='Docs/Members[@name="IsValidCpf"]/*'/>
        public static bool IsValidCpf(this string source) {

            // Sai do método caso a string seja inválida.
            if (string.IsNullOrWhiteSpace(source) == true) { return false; }

            // Obtém apenas os números do cpf.
            string cpf = source.GetDigits();

            // Retorna false, caso o cpf não possua 11 dígitos.
            if (cpf.Length != 11 || cpf.Distinct().Count() == 1) { return false; }

            // Armazena os números do CPF sem, os dígitos verificadores.
            string tempCpf = cpf.Substring(0, 9);

            // Calcula o primeiro dígito verificador.
            string digit = CalculateCpfDigit(tempCpf);

            // Calcula o segundo dígito verificador.
            digit += CalculateCpfDigit(tempCpf + digit);

            return cpf.EndsWith(digit);
        }

        /// <summary>
        /// Checks if the current string is a valid CNPJ.
        /// </summary>
        /// <param name="source">String to be checked.</param>
        /// <returns>Return true if the string is a valid CNPJ.</returns>
        /// <include file='Samples/StringExtensions.xml' path='Docs/Members[@name="IsValidCnpj"]/*'/>
        public static bool IsValidCnpj(this string source) {

            // Sai do método caso a string seja inválida.
            if (string.IsNullOrWhiteSpace(source) == true) { return false; }

            // Obtém apenas os números do cnpj.
            string cnpj = source.GetDigits();

            // Retorna false, caso o cnpj não possua 11 dígitos.
			if (cnpj.Length != 14 || cnpj.Distinct().Count() == 1) { return false; }

            // Armazena os números do CNPJ sem, os dígitos verificadores.
            string tempCnpj = cnpj.Substring(0, 12);

            // Calcula o primeiro dígito verificador.
            string digit = CalculateCnpjDigit(tempCnpj);

            // Calcula o segundo dígito verificador.
            digit += CalculateCnpjDigit(tempCnpj + digit);

            return cnpj.EndsWith(digit);
        }

        private static string CalculateCpfDigit(string source) {

            int sum = 0;

            // Processa os 9 primeiros dígitos do cpf.
            for (int i = 0; i < source.Length; i++) { sum += int.Parse(source[i].ToString()) * (source.Length + 1 - i); }

            // Armazena o resto da divisão por 11.
            int remainder = sum % 11;

            // Valores menores que 2 são convertidos para zero.
            remainder = (remainder < 2) ? 0 : 11 - remainder;

            // Retorna o dígito verificador.
            return remainder.ToString();
        }

        private static string CalculateCnpjDigit(string source) {

            int[] multiplier = new int[] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            if (source.Length == 13) { multiplier = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 }; }

            int sum = 0;

            // Processa os 9 primeiros dígitos do cpf.
            for (int i = 0; i < source.Length; i++) { sum += int.Parse(source[i].ToString()) * multiplier[i]; }

            // Armazena o resto da divisão por 11.
            int remainder = sum % 11;

            // Valores menores que 2 são convertidos para zero.
            remainder = (remainder < 2) ? 0 : 11 - remainder;

            // Retorna o dígito verificador.
            return remainder.ToString();
        }

        /// <summary>
        /// Checks if the current string has a valid email address format.
        /// </summary>
        /// <param name="source">String to be checked.</param>
        /// <returns>Return true if the string has a valid email address format.</returns>
        /// <include file='Samples/StringExtensions.xml' path='Docs/Members[@name="IsValidEmailAddress"]/*'/>
        public static bool IsValidEmailAddress(this string source) {

            // Verifica se foi especificado alguma string a ser validada.
            if (string.IsNullOrWhiteSpace(source) == true) { return false; }

            // Expressão de validação do email.
            string pattern = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))\z";

            // Instancia o processador de expressões regulares.
            Regex regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            // Retorna true caso o email seja válido.
            return regex.IsMatch(source);
        }

        /// <summary>
        /// Encrypts a string using the provided secretKey.
        /// </summary>
        /// <param name="source">String to be encrypted.</param>
        /// <param name="secretKey">The secret key to be used for calculate the encription.</param>
        /// <returns>Returns the encrypted hexadecimal string.</returns>
        public static string Encrypt(this string source, string secretKey) {

            // Verifica se a string foi especificada.
            if (string.IsNullOrWhiteSpace(source) == true) { return source; }

            // Verifica se a SecretKey foi especificada.
            if (string.IsNullOrWhiteSpace(secretKey) == true) { throw new ArgumentNullException("secretKey"); }

            // Array de bytes que conterá as informações da string.
            byte[] inputByteArray = UTF8Encoding.UTF8.GetBytes(source);

            // Gera o hash da chave.
            string hashedEncryptionKey = CalculateMd5(secretKey, secretKey);

            // Converte a chave de criptografia para uma array de bytes.
            byte[] keyArray = System.Text.Encoding.UTF8.GetBytes(hashedEncryptionKey.Substring(0, 24));

            TripleDESCryptoServiceProvider cryptoServiceProvider = new TripleDESCryptoServiceProvider();

            cryptoServiceProvider.Key = keyArray;
            cryptoServiceProvider.Mode = CipherMode.ECB;
            cryptoServiceProvider.Padding = PaddingMode.PKCS7;

            ICryptoTransform cryptoTransform = cryptoServiceProvider.CreateEncryptor();

            byte[] resultArray = cryptoTransform.TransformFinalBlock(inputByteArray, 0, inputByteArray.Length);

            cryptoServiceProvider.Clear();

            // Retorna a string hexadecimal para representar o hash.
            return ByteArrayToHexString(resultArray);
        }

        /// <summary>
        /// Decrypts a string using the provided secretKey.
        /// </summary>
        /// <param name="source">String to be decrypted.</param>
        /// <param name="secretKey">The secret key used in the encrypt process.</param>
        /// <returns>Returns an UTF8 decrypted string.</returns>
        public static string Decrypt(this string source, string secretKey) {

            // Verifica se a string foi especificada.
            if (string.IsNullOrWhiteSpace(source) == true) { return source; }

            // Verifica se a SecretKey foi especificada.
            if (string.IsNullOrWhiteSpace(secretKey) == true) { throw new ArgumentNullException("secretKey"); }

            // Array de bytes que conterá as informações da string.
            byte[] inputByteArray = HexStringToByteArray(source);

            // Gera o hash da chave.
            string hashedEncryptionKey = CalculateMd5(secretKey, secretKey);

            // Converte a chave de criptografia para uma array de bytes.
            byte[] keyArray = System.Text.Encoding.UTF8.GetBytes(hashedEncryptionKey.Substring(0, 24));

            TripleDESCryptoServiceProvider cryptoServiceProvider = new TripleDESCryptoServiceProvider();

            cryptoServiceProvider.Key = keyArray;
            cryptoServiceProvider.Mode = CipherMode.ECB;
            cryptoServiceProvider.Padding = PaddingMode.PKCS7;

            ICryptoTransform cryptoTransform = cryptoServiceProvider.CreateDecryptor();

            byte[] resultArray = cryptoTransform.TransformFinalBlock(inputByteArray, 0, inputByteArray.Length);

            cryptoServiceProvider.Clear();

            return UTF8Encoding.UTF8.GetString(resultArray);
        }

		/// <summary>
		/// Calculates the MD5 for a string.
		/// </summary>
		/// <param name="source">Source string to generate the MD5 from.</param>
		/// <returns>Returns the hexadecimal MD5 hash of the string.</returns>
		public static string CalculateMd5(this string source) {

			// Verifica se a string foi especificada.
			if (string.IsNullOrWhiteSpace(source) == true) { return source; }

			// Converte a mensagem para uma array de bytes.
			byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(source);

			MD5 md5 = MD5.Create();			

			// Calcula o hash da mensagem recebida.
			byte[] hashedMessageBytes = md5.ComputeHash(messageBytes);

			//Convertendo para Base64String
			return ByteArrayToHexString(hashedMessageBytes);
		}

        /// <summary>
        /// Calculates the MD5 for a string.
        /// </summary>
        /// <param name="source">Source string to generate the MD5 from.</param>
        /// <param name="secretKey">The secret key to be used for MD5 calculation.</param>
        /// <returns>Returns the MD5 string.</returns>
        /// <exception cref="ArgumentNullException">The secretKey was not specified.</exception>
        public static string CalculateMd5(this string source, string secretKey) {

            // Verifica se a string foi especificada.
            if (string.IsNullOrWhiteSpace(source) == true) { return source; }

            // Verifica se a SecretKey foi especificada.
            if (string.IsNullOrWhiteSpace(secretKey) == true) { throw new ArgumentNullException("secretKey"); }

            // Converte a secretKey em um array de bytes.
            byte[] keyBytes = System.Text.Encoding.UTF8.GetBytes(secretKey);

            // Converte a mensagem para uma array de bytes.
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(source);

            HMACMD5 hmac = new HMACMD5(keyBytes);

            // Calcula o hash da mensagem recebida.
            byte[] hashedMessageBytes = hmac.ComputeHash(messageBytes);

            // Retorna a string hexadecimal para representar o hash.
            return ByteArrayToHexString(hashedMessageBytes);
        }

        /// <summary>
        /// Calculates the hash from a string, using the SHA512 algoritm.
        /// </summary>
        /// <param name="source">Source string to generate the hash.</param>
        /// <param name="secretKey">Secret key used to generate the hash.</param>
        /// <returns>Returns the generated hashed string, or null, if the string is not valid.</returns>
        /// <exception cref="ArgumentNullException">The secretKey was not specified.</exception>
        public static string CalculateSha512(this string source, string secretKey) {

            // Verifica se a string foi especificada.
            if (string.IsNullOrWhiteSpace(source) == true) { return source; }

            // Verifica se a SecretKey foi especificada.
            if (string.IsNullOrWhiteSpace(secretKey) == true) { throw new ArgumentNullException("secretKey"); }

            // Converte a secretKey em um array de bytes.
            byte[] secretKeyBytes = System.Text.Encoding.UTF8.GetBytes(secretKey);

            // Instancia o HMAC a ser utilizado para gerar o hash.
            HMACSHA512 hmac = new HMACSHA512(secretKeyBytes);

            // Retorna o hash da string.
            return CalculateHash(source, hmac);
        }

        /// <summary>
        /// Calculates the hash from a string, using the SHA384 algoritm.
        /// </summary>
        /// <param name="source">Source string to generate the hash.</param>
        /// <param name="secretKey">Secret key used to generate the hash.</param>
        /// <returns>Returns the generated hashed string, or null, if the string is not valid.</returns>
        /// <exception cref="ArgumentNullException">The secretKey was not specified.</exception>
        public static string CalculateSha384(this string source, string secretKey) {

            // Verifica se a string foi especificada.
            if (string.IsNullOrWhiteSpace(source) == true) { return source; }

            // Verifica se a SecretKey foi especificada.
            if (string.IsNullOrWhiteSpace(secretKey) == true) { throw new ArgumentNullException("secretKey"); }

            // Converte a secretKey em um array de bytes.
            byte[] secretKeyBytes = System.Text.Encoding.UTF8.GetBytes(secretKey);

            // Instancia o HMAC a ser utilizado para gerar o hash.
            HMACSHA384 hmac = new HMACSHA384(secretKeyBytes);

            // Retorna o hash da string.
            return CalculateHash(source, hmac);
        }

        /// <summary>
        /// Calculates the hash from a string, using the SHA256 algoritm.
        /// </summary>
        /// <param name="source">Source string to generate the hash.</param>
        /// <param name="secretKey">Secret key used to generate the hash.</param>
        /// <returns>Returns the generated hashed string, or null, if the string is not valid.</returns>
        /// <exception cref="ArgumentNullException">The secretKey was not specified.</exception>
        public static string CalculateSha256(this string source, string secretKey) {

            // Verifica se a string foi especificada.
            if (string.IsNullOrWhiteSpace(source) == true) { return source; }

            // Verifica se a SecretKey foi especificada.
            if (string.IsNullOrWhiteSpace(secretKey) == true) { throw new ArgumentNullException("secretKey"); }

            // Converte a secretKey em um array de bytes.
            byte[] secretKeyBytes = System.Text.Encoding.UTF8.GetBytes(secretKey);

            // Instancia o HMAC a ser utilizado para gerar o hash.
            HMACSHA256 hmac = new HMACSHA256(secretKeyBytes);

            // Retorna o hash da string.
            return CalculateHash(source, hmac);
        }

        /// <summary>
        /// Calculates the hash from a string, using the SHA1 algoritm.
        /// </summary>
        /// <param name="source">Source string to generate the hash.</param>
        /// <param name="secretKey">Secret key used to generate the hash.</param>
        /// <returns>Returns the generated hashed string, or null, if the string is not valid.</returns>
        /// <exception cref="ArgumentNullException">The secretKey was not specified.</exception>
        public static string CalculateSha1(this string source, string secretKey) {

            // Verifica se a string foi especificada.
            if (string.IsNullOrWhiteSpace(source) == true) { return source; }

            // Verifica se a SecretKey foi especificada.
            if (string.IsNullOrWhiteSpace(secretKey) == true) { throw new ArgumentNullException("secretKey"); }

            // Converte a secretKey em um array de bytes.
            byte[] secretKeyBytes = System.Text.Encoding.UTF8.GetBytes(secretKey);

            // Instancia o HMAC a ser utilizado para gerar o hash.
            HMACSHA1 hmac = new HMACSHA1(secretKeyBytes);

            // Retorna o hash da string.
            return CalculateHash(source, hmac);
        }

        /// <summary>
        /// Calculate the hash from a string, using  the provided hmac instance.
        /// </summary>
        /// <param name="source">String to be hashed.</param>
        /// <param name="hmac">Hmac instance to be used to calculate the hash.</param>
        /// <returns>Returns the hashed string.</returns>
        private static string CalculateHash(string source, HMAC hmac) {

            // Converte a mensagem para uma array de bytes.
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(source);

            // Calcula o hash da mensagem recebida.
            byte[] hashedMessage = hmac.ComputeHash(messageBytes);

            // Retorna o hash da string.
            return ByteArrayToHexString(hashedMessage);
        }

        /// <summary>
        /// Converts an hexadecimal string to an byte array,
        /// </summary>
        /// <param name="source">Hexadecimal string to be converted.</param>
        /// <returns>Returns the byte array represented by the source string, or null, if the string is not specified.</returns>
        private static byte[] HexStringToByteArray(string source) {

            // Cria uma lista de inteiros, para conter cada caractere da string recebida.
            return Enumerable.Range(0, source.Length)
                // Pega cada caracter par, pois em strings hexadecimais, cada byte possui 2 caracteres: o par e o ímpar subsequente.
                             .Where(p => p % 2 == 0)
                // Converte os dois caracteres que estão em haxadecimal, para um byte.
                             .Select(p => Convert.ToByte(source.Substring(p, 2), 16))
                // Converte o resultado para um array de bytes a ser retornado.
                             .ToArray();
        }

        /// <summary>
        /// Converts a byte array to an hexadecimal string.
        /// </summary>
        /// <param name="source">Byte array to be converted.</param>
        /// <returns>Returns de hexadecimal string represented by the byte array, or null, if the byte array is not specified.</returns>
        private static string ByteArrayToHexString(byte[] source) {

            string sbinary = string.Empty;

            // Converte o array de bytes na string a ser retornada.
            for (int i = 0; i < source.Length; i++) { sbinary += source[i].ToString("X2"); }

            return sbinary;
        }

        /// <summary>
        /// Formats a string as CNPJ (00.000.000/0001-00). If the value is not a valid CNPJ, returns the original string.
        /// </summary>
        /// <param name="value">String to be formatted.</param>
        /// <returns>Returns the string with CNPJ format.</returns>
        public static string AsCnpj(this string value) {

            // Caso a string seja inválida, retorna o valor original.
            if (string.IsNullOrEmpty(value) == true) { return value; }

            // Obtém apenas os dígitos da string.
            char[] rawData = value.Where(char.IsDigit).ToArray();

            // Retorna a string original caso não seja um cnpj válido.
            if (rawData.Length != 14) { return value; }

            StringBuilder stringBuilder = new StringBuilder();

            // Executa a formatação para CNPJ.
            for (int i = 0; i < rawData.Length; i++) {
                stringBuilder.Append(rawData[i]);

                if (i == 1 || i == 4) { stringBuilder.Append('.'); }
                else if (i == 7) { stringBuilder.Append('/'); }
                else if (i == 11) { stringBuilder.Append('-'); }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Formats a string as CPF (999.999.999-99). If the value is not a valid CPF, returns the original string.
        /// </summary>
        /// <param name="value">String to be formatted.</param>
        /// <returns>Returns the string with CPF format.</returns>
        public static string AsCpf(this string value) {

            // Caso a string seja inválida, retorna o valor original.
            if (string.IsNullOrEmpty(value) == true) { return value; }

            // Obtém apenas os dígitos da string.
            char[] rawData = value.Where(char.IsDigit).ToArray();

            // Retorna a string original caso não seja um cpf válido.
            if (rawData.Length != 11) { return value; }

            StringBuilder stringBuilder = new StringBuilder();

            // Executa a formatação para CPF.
            for (int i = 0; i < rawData.Length; i++) {
                stringBuilder.Append(rawData[i]);
                if (i == 2 || i == 5) { stringBuilder.Append('.'); }
                else if (i == 8) { stringBuilder.Append('-'); }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Formats a string as phone number.
        /// </summary>
        /// <param name="value">String to be formatted.</param>
        /// <returns>Returns the string with phone number format.</returns>
        public static string AsPhoneNumber(this string value) {

            // Caso a string seja inválida, retorna o valor original.
            if (string.IsNullOrEmpty(value) == true) { return value; }

            // Obtém apenas os dígitos da string.
            List<char> rawData = value.Where(char.IsDigit).ToList();

            // Retorna a string original caso não seja um telefone válido.
            if (rawData.Count < 7) { return value; }

            int digitsCount = rawData.Count;

            // Converte o tel do formato 552133335555 para 55 (21) 33335555
            if (digitsCount >= 12 && digitsCount < 14) {

                int offset = (digitsCount == 12 && rawData[3] == '9') ? -1 : 0;

                rawData.Insert(2 + offset, ' ');
                rawData.Insert(3 + offset, '(');
                rawData.Insert(6 + offset, ')');
                rawData.Insert(7 + offset, ' ');
            }

            // Converte o tel do formato 2133335555 para (21) 33335555
            if (digitsCount >= 10 && digitsCount < 12) {
                rawData.Insert(0, '(');
                rawData.Insert(3, ')');
                rawData.Insert(4, ' ');
            }

            // Adiciona o separador antes dos 4 últimos dígitos.
            rawData.Insert(rawData.Count - 4, '-');

            return new string(rawData.ToArray());
        }

        /// <summary>
        /// Formats a string as ZipCode. If the value is not a valid ZipCode, returns the original string.
        /// </summary>
        /// <param name="value">String to be formatted.</param>
        /// <returns>Returns the formated string as ZipCode.</returns>
        public static string AsZipCode(this string value) {

            // Caso a string seja inválida, retorna o valor original.
            if (string.IsNullOrEmpty(value) == true) { return value; }

            // Obtém apenas os dígitos da string.
            List<char> rawData = value.Where(char.IsDigit).ToList();

            // Caso não possua 8 dígitos, retorna o valor original.
            if (rawData.Count != 8) { return value; }

            rawData.Insert(5, '-');

            return new string(rawData.ToArray());
        }
    }

    /// <summary>
    /// Enumerates all the available mask formats.
    /// </summary>
    public enum StringMaskFormat {

        /// <summary>
        /// Masks all characters.
        /// </summary>
        Password = 0,

        /// <summary>
        /// Partially mask the string in a creditcard format, allowing only the last 4 digits to be seen. If the string length is less than 4, all the string is masked.
        /// </summary>
        CreditCard = 1,

        /// <summary>
        /// Partially mask the string in a creditcard format, allowing the first 6 digits and the last 4 digits to be seen. The string length must be greater than 13, otherwise the CreditCard format is applied.
        /// </summary>
        CreditCardExtended = 2,
    }
}
