using System.Collections;
using System.Text;

namespace Dlp.Framework {

    /// <summary>
    /// Collections extension methods.
    /// </summary>
    public static class CollectionExtensions {

        /// <summary>
        /// Converts a collection into a string, where each element is separated with a comma, by default.
        /// </summary>
        /// <param name="source">Collection to be converted.</param>
        /// <param name="separator">Char separator. The defalt separator is comma.</param>
        /// <param name="surroundWith">Specify the surrounding chars for the elements. For example.: single quotation mark "'": 'element1','element2',...</param>
        /// <returns>Returns a new string containing all the elements received, or null, if the source collection is null.</returns>
        /// <include file='Samples/CollectionExtensions.xml' path='Docs/Members[@name="AsString"]/*'/>
        public static string AsString(this IEnumerable source, char separator = ',', string surroundWith = null) {

            // Verifica se a coleção foi especificada.
            if (source == null) { return null; }

            StringBuilder stringBuilder = new StringBuilder();

            // Converte cada elemento para string, adicionando o separador.
            foreach (object t in source) { stringBuilder.AppendFormat("{0}{1}{0}{2}", surroundWith, t, separator); }

            // Retorna a string, removendo o separador extra no final.
            return stringBuilder.ToString().TrimEnd(separator);
        }
    }
}