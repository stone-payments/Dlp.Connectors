using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Dlp.Framework {

    /// <summary>
    /// Serialization utility for Xml and Binary formats.
    /// </summary>
    public static class Serializer {

        /// <summary>
        /// Serialize the object to a byte array. The object to be serialized must have the [Serializable] attribute.
        /// </summary>
        /// <param name="source">Object to be serialized.</param>
        /// <returns>Retorna um array de bytes que representa o objeto. Retorna nulo, caso o objeto especificado seja nulo.</returns>
        /// <exception cref="System.Runtime.Serialization.SerializationException">The specified object does not have the [Serializable] attribute.</exception>
        public static byte[] BinarySerialize(object source) {

            // Sai do método caso o objeto a ser serializado seja nulo.
            if (source == null) { return null; }

            byte[] byteArray;

            // Formatter utilizado para gerenciar a serialização binária.
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            // Serializa o valor do item a ser adicionano no cache.
            using (MemoryStream stream = new MemoryStream()) {

                binaryFormatter.Serialize(stream, source);
                byteArray = stream.ToArray();
            }

            return byteArray;
        }

        /// <summary>
        /// Deserialize a byte array to a new instance of type T.
        /// </summary>
        /// <typeparam name="T">Type of the instance to be returned.</typeparam>
        /// <param name="source">Byte array to be deserialized.</param>
        /// <returns>Return a new instance of type T with the deserialized data, or default(T), if the byte array is null.</returns>
        public static T BinaryDeserialize<T>(byte[] source) {

            if (source == null) { return default(T); }

            // Formatter utilizado para gerenciar a serialização binária.
            BinaryFormatter binaryFormatter = new BinaryFormatter();

            // Objeto que conterá o request recebido.
            object obj;

            // Desserializa o objeto recebido.
            using (MemoryStream stream = new MemoryStream(source)) { obj = binaryFormatter.Deserialize(stream); }

            return (T)obj;
        }

        /// <summary>
        /// Deserializes a XML string to a new instance of type T.
        /// </summary>
        /// <typeparam name="T">Type of the instance to be returned.</typeparam>
        /// <param name="source">XML string to be deserialized.</param>
        /// <param name="encoding">Encoding to be used for deserialization. Default value: Encoding.UTF8.</param>
        /// <returns>Return a new instance of type T with the deserialized data, or default(T), if the XML string is null.</returns>
        public static T XmlDeserialize<T>(string source, Encoding encoding = null) where T : class {

            object result = XmlDeserialize(typeof(T), source, encoding);

            if (result == null) { return default(T); }

            return (T)result;
        }

        /// <summary>
        /// Deserializes a XML string to an object instance.
        /// </summary>
        /// <param name="returnType">Type of the instance to be returned.</param>
        /// <param name="source">XML string to be deserialized.</param>
        /// <param name="encoding">Encoding to be used for deserialization. Default value: Encoding.UTF8.</param>
        /// <returns>Return a new instance of type T with the deserialized data, or default(T), if the XML string is null.</returns>
        public static object XmlDeserialize(Type returnType, string source, Encoding encoding = null) {

            // Verifica se a string foi especificada.
            if (string.IsNullOrWhiteSpace(source) == true) { return null; }

            // Verifica se foi especificado algum encoding.
            if (encoding == null) { encoding = Encoding.UTF8; }

            // Instancia o serializador.
            XmlSerializer xmlSerializer = new XmlSerializer(returnType);

            // Converte a string para array de bytes.
            byte[] bytes = source.GetBytes(encoding);

            // Cria um stream de memória para que o array possa ser desserializado.
            using (MemoryStream memoryStream = new MemoryStream(bytes)) {

                // Retorna o objeto desserializado.
                return xmlSerializer.Deserialize(memoryStream);
            }
        }

        /// <summary>
        /// Serialize an object to a XML string.
        /// </summary>
        /// <param name="source">Object to be converted.</param>
        /// <param name="indent">Set to true to generate a indented XML string. Default: false.</param>
        /// <param name="encoding">Encoding to be used for serialization. Default value: Encoding.UTF8.</param>
        /// <returns>Returns the serialized string, or null, if the source object was not suplied.</returns>
        public static string XmlSerialize(object source, bool indent = false, Encoding encoding = null) {

            // Verifica se o objeto foi especificado.
            if (source == null) { return null; }

            // Verifica se foi especificado algum encoding.
            if (encoding == null) { encoding = Encoding.UTF8; }

            // Instancia o serializador.
            XmlSerializer xmlSerializer = new XmlSerializer(source.GetType());

            // Cria um stream de memória para que o objeto possa ser serializado.
            using (MemoryStream memoryStream = new MemoryStream()) {

                // Configurações da serialização.
                XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, encoding);

                // Verifica se foi especificada indentação para o xml.
                if (indent == true) {

                    // Define o formato de identação do xml gerado.
                    xmlTextWriter.Formatting = Formatting.Indented;

                    // 1 caractere de indentação por nível.
                    xmlTextWriter.Indentation = 1;

                    // 9 representa o caractere de tabulação.
                    xmlTextWriter.IndentChar = Convert.ToChar(9);
                }

                // Serializa o objeto para a memória.
                xmlSerializer.Serialize(xmlTextWriter, source);

                // Retorna a string serializada da memória.
                return encoding.GetString(memoryStream.ToArray());
            }
        }

        /// <summary>
        /// Serializes an object to a JSON string format.
        /// </summary>
        /// <param name="source">Object to be serialized.</param>
        /// <param name="encoding">Encoding to be used for serialization. Default value: Encoding.UTF8.</param>
        /// <returns>Returns the serialized string, or null, if the source object was not suplied.</returns>
        public static string JsonSerialize(object source, Encoding encoding = null) {

            // Sai do método caso não exista informação a ser serializada.
            if (source == null) { return null; }

            // Verifica se foi especificado algum encoding.
            if (encoding == null) { encoding = Encoding.UTF8; }

            // Instancia o objeto responsável pela serialização.
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(source.GetType());

            // Instancia o memoryStream que conterá o objeto serializado.
            using (MemoryStream memoryStream = new MemoryStream()) {

                // Serializa o objeto para a memória.
                jsonSerializer.WriteObject(memoryStream, source);

                // Converte o array de bytes serializado para a string a ser retornada.
                return encoding.GetString(memoryStream.ToArray());
            }
        }

        /// <summary>
        /// Deserializes a JSON string to an instance of type T.
        /// </summary>
        /// <typeparam name="T">Type of the instance to be returned.</typeparam>
        /// <param name="source">JSON string to be deserialized.</param>
        /// <param name="encoding">Encoding to be used for deserialization. Default value: Encoding.UTF8.</param>
        /// <returns>Return a new instance of type T with the deserialized data, or default(T), if the JSON string is null.</returns>
        public static T JsonDeserialize<T>(string source, Encoding encoding = null) where T : class {

            // Desserializa o objeto.
            object result = JsonDeserialize(typeof(T), source, encoding);

            // Caso o objeto não exista, returna default(T).
            if (result == null) { return default(T); }

            return (T)result;
        }

        /// <summary>
        /// Deserializes a JSON string to an object instance.
        /// </summary>
        /// <param name="returnType">Type of the instance to be returned.</param>
        /// <param name="source">JSON string to be deserialized.</param>
        /// <param name="encoding">Encoding to be used for deserialization. Default value: Encoding.UTF8.</param>
        /// <returns>Return a new instance of type T with the deserialized data, or default(T), if the JSON string is null.</returns>
        public static object JsonDeserialize(Type returnType, string source, Encoding encoding = null) {

            // Verifica se o objeto foi especificado.
            if (source == null) { return null; }

            // Verifica se foi especificado algum encoding. Caso negativo, define UTF8.
            if (encoding == null) { encoding = Encoding.UTF8; }

            // Instancia o objeto responsável pela serialização.
            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(returnType);

            // Armazena o array de bytes do objeto na memória.
            using (MemoryStream memoryStream = new MemoryStream(encoding.GetBytes(source))) {

                // Realiza a deserialização da string para o objeto a ser retornado.
                return jsonSerializer.ReadObject(memoryStream);
            }
        }
    }
}
