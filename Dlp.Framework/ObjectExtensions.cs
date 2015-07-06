using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;

namespace Dlp.Framework {

    /// <summary>
    /// Object extension methods.
    /// </summary>
    public static class ObjectExtensions {

        /// <summary>
        /// Creates a new instance of the current object without references to the original.
        /// </summary>
        /// <typeparam name="T">Type of the object to be cloned.</typeparam>
        /// <param name="source">Object to be cloned.</param>
        /// <returns>Return a new instance of T filed with the data of the original object.</returns>
        /// <include file='Samples/ObjectExtensions.xml' path='Docs/Members[@name="Clone"]/*'/>
        public static T Clone<T>(this T source) where T : class, new() {

            // Sai do método caso o objeto a ser clonado seja nulo.
            if (source == null) { return default(T); }

            // Serializa o objeto original.
            string serializedData = Serializer.JsonSerialize(source);

            // Desserializa o objeto, gerando uma nova instancia.
            T result = Serializer.JsonDeserialize<T>(serializedData);

            return result;
        }

        /// <summary>
        /// Creates a new instance of the current object without references to the original.
        /// </summary>
        /// <param name="source">Object to be cloned.</param>
        /// <returns>Return a new independent instance of the original object.</returns>
        public static object Clone(this object source) {

            // Armazena o nome do tipo a ser serializado.
            string typeName = source.GetType().AssemblyQualifiedName;

            // Armazena o tipo do objeto.
            Type requestType = Type.GetType(typeName);

            // Serializa o objeto para uma string.
            string serialized = Serializer.JsonSerialize(source);

            // Inicializa o serializador json.
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(requestType);

            // Converte a string para array de bytes.
            byte[] bytes = serialized.GetBytes(Encoding.UTF8);

            // Cria um stream de memória para que o array possa ser desserializado.
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(bytes)) {

                // Retorna o objeto desserializado.
                return Convert.ChangeType(serializer.ReadObject(memoryStream), requestType);
            }
        }

        /// <summary>
        /// Compares all the properties that are different between two objects.
        /// </summary>
        /// <typeparam name="T">Type of the object to be verified.</typeparam>
        /// <param name="firstObject">First object to be checked.</param>
        /// <param name="secondObject">Second object to be checked.</param>
        /// <returns>Returns a collection of DiffProperty containing all the properties that does not match.</returns>
        public static IEnumerable<DiffProperty> DiffProperties<T>(this T firstObject, T secondObject) where T : new() {

            // Sai do método caso os objetos não tenham sido especificados.
            if (firstObject == null || secondObject == null) { yield break; }

            // Armazena todas as propriedades públicas do objeto 1.
            PropertyInfo[] firstObjectProperties = firstObject.GetType().GetProperties();

            // Armazena todas as propriedades públicas do objeto 2.
            PropertyInfo[] secondObjectProperties = secondObject.GetType().GetProperties();

            foreach (PropertyInfo firstPropertyInfo in firstObjectProperties) {

                // Verifica se existe uma propriedade 
                PropertyInfo secondPropertyInfo = secondObjectProperties.FirstOrDefault(p => p.Name == firstPropertyInfo.Name);

                object firstObjectValue = firstPropertyInfo.GetValue(firstObject, null);
                object secondObjectValue = secondPropertyInfo.GetValue(secondObject, null);

                string parsedFirstObjectValue = (firstObjectValue != null) ? Convert.ToString(firstObjectValue, CultureInfo.InvariantCulture) : null;
                string parsedSecondObjectValue = (secondObjectValue != null) ? Convert.ToString(secondObjectValue, CultureInfo.InvariantCulture) : null;

                // Verifica se os valores das propriedades são diferentes.
                if (parsedFirstObjectValue != parsedSecondObjectValue) {

                    DiffProperty diffProperty = new DiffProperty();

                    diffProperty.PropertyName = secondPropertyInfo.Name;
                    diffProperty.FirstValue = parsedFirstObjectValue;
                    diffProperty.SecondValue = parsedSecondObjectValue;

                    yield return diffProperty;
                }
            }
        }
    }

    /// <summary>
    /// Class that contains the different values of a property.
    /// </summary>
    public sealed class DiffProperty {

        /// <summary>
        /// Instantiate the DiffProperty class.
        /// </summary>
        public DiffProperty() { }

        /// <summary>
        /// The name of the property that does not match.
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// Tha value of the property in the first object.
        /// </summary>
        public string FirstValue { get; set; }

        /// <summary>
        /// The value of the property in the second object.
        /// </summary>
        public string SecondValue { get; set; }
    }
}
