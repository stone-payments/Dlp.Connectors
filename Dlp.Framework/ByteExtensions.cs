using System.Text;

namespace Dlp.Framework {

    /// <summary>
    /// Byte array extension methods.
    /// </summary>
    public static class ByteExtensions {

        /// <summary>
        /// Converts a byte array to string.
        /// </summary>
        /// <param name="source">Byte array to be converted.</param>
        /// <param name="encoding">Encoding to be used for conversion. Default value: Encoding.UTF8.</param>
        /// <returns>Return a new string generated from byte array, or null, if a invalid byte array is received.</returns>
        /// <include file='Samples/ByteExtensions.xml' path='Docs/Members/*'/>
        public static string GetString(this byte[] source, Encoding encoding = null) {

            // Verifica se o array de bytes foi especificado.
            if (source == null || source.Length == 0) { return null; }

            // Verifica se foi especificado algum encoding.
            if (encoding == null) { encoding = Encoding.UTF8; }

            return encoding.GetString(source);
        }
    }
}