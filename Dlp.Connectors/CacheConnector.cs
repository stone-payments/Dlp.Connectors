using Dlp.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dlp.Connectors {

    public sealed class CacheConnector : ICacheConnector, IDisposable {

        public CacheConnector(Guid accessKey, string serverAddress, short port = 10100, short bufferSize = 512) {

            this.AccessKey = accessKey;
            this.ServerAddress = serverAddress;
            this.Port = port;
            this._callingAssembly = Assembly.GetCallingAssembly().GetName().Name;
            _bufferSize = bufferSize;
        }

        /// <summary>
        /// Armazena o nome da aplicação que esta chamando o serviço de cache.
        /// </summary>
        private readonly string _callingAssembly;

        /// <summary>
        /// Obtém ou define o tamanho do buffer a ser utilizado na comunicação com o serviço de cache.
        /// </summary>
        private static short _bufferSize;

        /// <summary>
        /// Obtém o endereço do servidor de cache.
        /// </summary>
        private string ServerAddress { get; set; }

        /// <summary>
        /// Obtém a porta de conexão na qual o cache será disponibilizado.
        /// </summary>
        private short Port { get; set; }

        /// <summary>
        /// Obtém a chave de acesso ao serviço de cache.
        /// </summary>
        private Guid AccessKey { get; set; }

        /// <summary>
        /// Obtém o socket utilizado para se comunicar com o serviço de cache.
        /// </summary>
        private Socket Handler = null;

        /// <summary>
        /// Connects to the cache service.
        /// </summary>
        public bool Connect() {

            try {
                // Sai do método caso já exista uma conexão aberta.
                if (this.IsConnected() == true) { return true; }

                // Obtém as informações desta máquina, para configurar o endpoint do socket.
                IPHostEntry ipHostEntry = Dns.GetHostEntry(this.ServerAddress);

                // Armazena o ip do servidor.
                IPAddress ipAddress = ipHostEntry.AddressList[0];

                // Cria o endpoint que será utilizado para acessar o servidor.
                IPEndPoint remoteEndPoint = new IPEndPoint(ipAddress, this.Port);

                // Cria o socket TCP/IP.
                this.Handler = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                // Solicita a conexão com o serviço de cache.
                this.Handler.Connect(remoteEndPoint);

                // Converte a string a ser enviada para um array de bytes.
                byte[] byteData = Encoding.Default.GetBytes(this.AccessKey.ToString());

                // Envia a chave de acesso para validação no serviço.
                this.Handler.Send(byteData);

                byte[] receivedData = new byte[_bufferSize];

                // Armazena a quantidade de bytes recebidos.
                int receivedBytes = this.Handler.Receive(receivedData);

                // Converte a string a ser enviada para um array de bytes.
                string content = Encoding.Default.GetString(receivedData, 0, receivedBytes);

                // Retorna true caso o servidor tenha retornado 0 (zero).
                return content.Equals("0");
            }
            catch (Exception ex) {

                // TODO: Logar a exceção.
                string message = ex.Message;

                return false;
            }
        }

        /// <summary>
        /// Checks if the cache service is connected.
        /// </summary>
        /// <returns>Returns true if we are connected to the cache service.</returns>
        public bool IsConnected() {

            return (this.Handler != null && this.Handler.Connected == true);
        }

        /// <summary>
        /// Disconnects from the cache service.
        /// </summary>
        public void Disconnect() {

            // Caso o handler não exista, sai do método.
            if (this.Handler == null) { return; }

            this.Handler.Shutdown(SocketShutdown.Both);
            this.Handler.Close();
            this.Handler = null;
        }

        /// <summary>
        /// Adiciona um atualiza um item existente no cache.
        /// </summary>
        /// <typeparam name="T">Tipo do item a ser inserido/atualziado.</typeparam>
        /// <param name="key">Nome único no cache.</param>
        /// <param name="item">Objeto a ser inserido/atualizado.</param>
        /// <param name="itemExpirationInMinutes">Quanto tempo o item deverá permanecer no cache.</param>
        /// <returns>Retorna true caso a operação seja realizada com sucesso.</returns>
        public bool Set<T>(string key, T item, int itemExpirationInMinutes) where T : new() {

            try {
                // Verifica se o nome do item foi especificado.
                if (string.IsNullOrWhiteSpace(key) == true) { return false; }

                // Verifica se o objeto foi especificado.
                if (item == null) { return false; }

                // Obtém o array de bytes a ser enviado para o serviço de cache.
                string content = this.CreateBinaryRequest<T>(CacheOperationType.Set, key, item, itemExpirationInMinutes);

                SendAsync(this.Handler, content);

                return true;
            }
            catch (Exception ex) {

                // TODO: Logar a exceção.
                string message = ex.Message;

                return false;
            }
        }

        /// <summary>
        /// Obtém um item do cache a partir do nome.
        /// </summary>
        /// <typeparam name="T">Tipo do objeto a ser retornado.</typeparam>
        /// <param name="key">Nome do objeto no cache.</param>
        /// <returns>Retorna um objeto do tipo T, encontrado no cache, ou default(T), caso o item não exista.</returns>
        public T GetByKey<T>(string key) where T : class, new() {

            try {
                // Verifica se o nome do item foi especificado.
                if (string.IsNullOrWhiteSpace(key) == true) { return default(T); }

                // Cria a solicitação ao serviço de cache.
                string content = this.CreateBinaryRequest<T>(CacheOperationType.GetByKey, key, null, 0);

                byte[] result = Request(this.Handler, content);

                // Sai do método, caso o objeto não exista no cache.
                if (result == null || result.Length == 0 || result[0] == '\0') { return default(T); }

                string jsonObject = result.GetString();

                // Retorna o objeto.
                return Serializer.JsonDeserialize<T>(jsonObject, Encoding.Default);// Serializer.BinaryDeserialize<T>(result);
            }
            catch (Exception ex) {

                // TODO: Logar a exceção.
                string message = ex.Message;

                throw;
            }
        }

        /// <summary>
        /// Prepara o CacheItem para ser enviado para o serviço de cache.
        /// </summary>
        /// <param name="cacheOperationType">Objeto a ser preparado.</param>
        /// <param name="key"></param>
        /// <param name="item"></param>
        /// <param name="itemExpirationInMinutes"></param>
        /// <returns></returns>
        private string CreateBinaryRequest<T>(CacheOperationType cacheOperationType, string key, object item, int itemExpirationInMinutes) {

            string itemType = typeof(T).Name;

            StringBuilder dataContent = new StringBuilder();

            dataContent.AppendFormat("<Operation:{0}><EOL>", (int)cacheOperationType);
            dataContent.AppendFormat("<Type:{0}><EOL>", itemType);
            dataContent.AppendFormat("<Region:{0}><EOL>", _callingAssembly);
            dataContent.AppendFormat("<Key:{0}><EOL>", key);

            // Verifica se o tempo de expiração foi especificado.
            if (itemExpirationInMinutes > 0) { dataContent.AppendFormat("<Expiration:{0}><EOL>", itemExpirationInMinutes); }

            // Verifica se o item a ser armazenado foi especificado.
            if (item != null) {
                string serializedData = Serializer.JsonSerialize(item, Encoding.Default);
                byte[] itemBytes = serializedData.GetBytes(); //Serializer.BinarySerialize(item);
                dataContent.AppendFormat("<Value:{0}><EOL>", Encoding.Default.GetString(itemBytes));
            }

            dataContent.Append("<EOF>");

            return dataContent.ToString();
        }

        private static byte[] Request(Socket client, string data) {

            // Converte a string a ser enviada para um array de bytes.
            byte[] byteData = Encoding.Default.GetBytes(data);

            // Envia a requisição para o serviço de cache.
            client.Send(byteData);

            List<Byte> result = new List<byte>();

            // Buffer que armazenará os dados recebidos do serviço de cache.
            byte[] receivedData = new byte[_bufferSize];

            do {
                // Lê os dados recebidos.
                int receivedBytes = client.Receive(receivedData, receivedData.Length, SocketFlags.None);

                // Adiciona os dados recebidos na lista a ser retornada.
                result.AddRange(receivedData.Take(receivedBytes));

            } while (client.Available > 0);

            return result.ToArray();
        }

        private static void SendAsync(Socket client, string data) {

            // Converte a string a ser enviada para um array de bytes.
            byte[] byteData = Encoding.Default.GetBytes(data);

            // Inicia o envio da mensagem.
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private static void SendCallback(IAsyncResult ar) {

            try {
                // Obtém o socket utilizado na comunicação.
                Socket client = (Socket)ar.AsyncState;

                // Finaliza o envio da mensagem.
                int bytesSent = client.EndSend(ar);
            }
            catch (Exception ex) {

                // TODO: Logar a exceção.
                string message = ex.Message;
            }
        }

        internal enum CacheOperationType {

            /// <summary>
            /// Operação de inserção ou atualização de um item no cache.
            /// </summary>
            Set = 0,

            /// <summary>
            /// Operação de leitura de um item do cache.
            /// </summary>
            GetByKey = 10,

            /// <summary>
            /// Operação de leitura de todos os itens de um tipo específico.
            /// </summary>
            GetByType = 11,

            /// <summary>
            /// Operação de exclusão de um item do cache.
            /// </summary>
            Delete = 20
        }

        public void Dispose() {

            if (this.Handler != null) { this.Handler.Dispose(); }
        }
    }
}
