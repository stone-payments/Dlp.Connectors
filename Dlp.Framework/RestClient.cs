using System;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Framework {

    /// <summary>
    /// Enumerates the available HTTP verbs for REST communication.
    /// </summary>
    public enum HttpVerb {

        /// <summary>
        /// Defines a HTTP GET method.
        /// </summary>
        Get,

        /// <summary>
        /// Defines a HTTP POST method.
        /// </summary>
        Post,

        /// <summary>
        /// Defines a HTTP PUT method.
        /// </summary>
        Put,

        /// <summary>
        /// Defines a HTTP DELETE method.
        /// </summary>
        Delete,

        /// <summary>
        /// Defines a HTTP PATCH method.
        /// </summary>
        Patch
    }

    /// <summary>
    /// Enumerates the available formats for REST communication.
    /// </summary>
    public enum HttpContentType {

        /// <summary>
        /// Defines that the rest communication should be made with XML format.
        /// </summary>
        Xml,

        /// <summary>
        /// Defines that the rest communication should be made with JSON format.
        /// </summary>
        Json
    }

    /// <summary>
    /// Represents the response for a HttpWebRequest.
    /// </summary>
    /// <typeparam name="T">Type of the response of a HttpWebRequest.</typeparam>
    public sealed class WebResponse<T> {

        /// <summary>
        /// Initializes a new instance of the WebResponse class.
        /// </summary>
        public WebResponse() { }

        /// <summary>
        /// Gets the returned HttpStatusCode.
        /// </summary>
        public HttpStatusCode StatusCode { get; internal set; }

        /// <summary>
        /// Gets the returned data.
        /// </summary>
        public T ResponseData { get; internal set; }
    }

    /// <summary>
    /// REST utility for HTTP communication.
    /// </summary>
    public static class RestClient {

		/// <summary>
		/// Sends an Http request to the specified endpoint.
		/// </summary>
		/// <typeparam name="T">Type of the expected response. Ignored if http verb GET is used.</typeparam>
		/// <param name="dataToSend">Object containing the data to be sent in the request.</param>
		/// <param name="httpVerb">HTTP verb to be using when sending the data.</param>
		/// <param name="httpContentType">Content type of the transferred data.</param>
		/// <param name="destinationEndPoint">Endpoint where the request will be sent to.</param>
		/// <param name="headerCollection">Custom data to be added to the request header.</param>
		/// <param name="allowInvalidCertificate">When set to true, allows the request to be done even if the destination certificate is not valid.</param>
		/// <returns>Returns an WebResponse as a Task, containing the result of the request.</returns>
        public static WebResponse<T> SendHttpWebRequest<T>(object dataToSend, HttpVerb httpVerb, HttpContentType httpContentType, string destinationEndPoint, NameValueCollection headerCollection, bool allowInvalidCertificate = false) where T : class {

            // Verifica se o endpoint para onde a requisição será enviada foi especificada.
            if (string.IsNullOrWhiteSpace(destinationEndPoint)) { throw new ArgumentNullException("serviceEndpoint", "The serviceEndPoint parameter must not be null."); }

            // Cria a uri para onde a requisição será enviada.
            Uri destinationUri = new Uri(destinationEndPoint);

            // Instancia o objeto resposável pela comunicação.
            HttpWebRequest httpWebRequest = WebRequest.Create(destinationUri) as HttpWebRequest;

            // Define o verbo a ser utilizado na comunicação.
            httpWebRequest.Method = httpVerb.ToString().ToUpperInvariant();

            // Define o formato das mensagens.
            httpWebRequest.ContentType = httpWebRequest.Accept = (httpContentType == HttpContentType.Json) ? "application/json" : "application/xml";

            // Define o User-Agent direto no HttpWebRequest, ao invés de enviar como header.
            if (headerCollection != null && headerCollection.AllKeys.Contains("User-Agent") == true) {

                // Obtém o User-Agent especificado.
                httpWebRequest.UserAgent = headerCollection["User-Agent"];

                // Remove o User-Agent para que ele não seja adicionado ao header.
                headerCollection.Remove("User-Agent");
            }

            if (allowInvalidCertificate == true) {

                // Verifica se certificados inválidos devem ser aceitos.
                ServicePointManager.ServerCertificateValidationCallback = ((sender, certificate, chain, sslPolicyErrors) => true);

            }

            // Verifica se deverão ser enviados dados no header.
            if (headerCollection != null && headerCollection.Count > 0) {

                // Insere cada chave no header da requisição.
                foreach (string key in headerCollection.Keys) { httpWebRequest.Headers.Add(key, headerCollection[key].ToString()); }
            }

            // Verifica se foi especificada a informação a ser enviada.
            if (dataToSend != null) {

                // Serializa o objeto para o formato especificado.
                string serializedData = (httpContentType == HttpContentType.Json) ? Serializer.JsonSerialize(dataToSend) : Serializer.XmlSerialize(dataToSend);

                // Cria um array de bytes dos dados que serão enviados.
                byte[] byteData = UTF8Encoding.UTF8.GetBytes(serializedData);

                // Define o tamanho dos dados que serão enviados.
                httpWebRequest.ContentLength = byteData.Length;

                // Escreve os dados na stream do WebRequest.
                using (Stream stream = httpWebRequest.GetRequestStream()) {
                    stream.Write(byteData, 0, byteData.Length);
                }
            }

            // Inicializa o código de status http a ser retornado.
            HttpStatusCode responseStatusCode = HttpStatusCode.OK;

            // Variável que armazenará o resultado da requisição.
            string returnString = string.Empty;

            HttpWebResponse response = null;

            try {
                // Dispara a requisição e recebe o resultado.
                using (response = httpWebRequest.GetResponse() as HttpWebResponse) {

                    // Recupera a stream com a resposta da solicitação.
                    using (StreamReader streamReader = new StreamReader(response.GetResponseStream())) {

                        // Obtém a resposta com string.
                        returnString = streamReader.ReadToEnd();
                    }

                    responseStatusCode = response.StatusCode;
                }
            }
            catch (WebException ex) {

                // Obtém os dados da exceção.
                StreamReader stream = new StreamReader(ex.Response.GetResponseStream());

                // Converte a informação para string.
                returnString = stream.ReadToEnd();

                // Define o status da requisição como erro do servidor.
                responseStatusCode = HttpStatusCode.InternalServerError;
            }

            T returnValue = null;

            // Caso o tipo a ser retornado seja uma string, não executa a deserialização.
            if (typeof(T) == typeof(string)) { returnValue = returnString as T; }
            else {
                // Executa a deserialização adequada.
                returnValue = (httpContentType == HttpContentType.Json) ? Serializer.JsonDeserialize<T>(returnString) : Serializer.XmlDeserialize<T>(returnString);
            }

            // Cria o objeto contendo o resultado da requisição.
            return new WebResponse<T>() { StatusCode = responseStatusCode, ResponseData = returnValue };
        }

        /// <summary>
        /// Sends an Http request to the specified endpoint asyncrounously.
        /// </summary>
        /// <typeparam name="T">Type of the expected response.</typeparam>
        /// <param name="dataToSend">Object containing the data to be sent in the request. Ignored if http verb GET is used.</param>
        /// <param name="httpVerb">HTTP verb to be using when sending the data.</param>
        /// <param name="httpContentType">Content type of the transferred data.</param>
        /// <param name="destinationEndPoint">Endpoint where the request will be sent to.</param>
        /// <param name="headerCollection">Custom data to be added to the request header.</param>
        /// <param name="allowInvalidCertificate">When set to true, allows the request to be done even if the destination certificate is not valid.</param>
        /// <returns>Returns an WebResponse as a Task, containing the result of the request.</returns>
        public static async Task<WebResponse<T>> SendHttpWebRequestAsync<T>(object dataToSend, HttpVerb httpVerb, HttpContentType httpContentType, string destinationEndPoint, NameValueCollection headerCollection, bool allowInvalidCertificate = false) where T : class {

            // Verifica se o endpoint para onde a requisição será enviada foi especificada.
			if (string.IsNullOrWhiteSpace(destinationEndPoint)) { throw new ArgumentNullException("serviceEndpoint", "The serviceEndPoint parameter must not be null."); }

            return await Task.Run<WebResponse<T>>(() => {

                return SendHttpWebRequest<T>(dataToSend, httpVerb, httpContentType, destinationEndPoint, headerCollection, allowInvalidCertificate);
            });
        }
    }
}
