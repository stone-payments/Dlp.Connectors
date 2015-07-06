using System;
using System.Runtime.Serialization;

namespace Dlp.Authenticator.DataContracts.InternalContracts {

    [DataContract]
    internal sealed class InternalValidateApplicationRequest {

        public InternalValidateApplicationRequest() { }

        /// <summary>
        /// Chave de identificação da aplicação que esta fazendo a requisição.
        /// </summary>
        [DataMember]
        public Guid ApplicationKey { get; set; }

        /// <summary>
        /// Chave de identificação da aplicação a ser validada.
        /// </summary>
        [DataMember]
        public Guid ClientApplicationKey { get; set; }

        /// <summary>
        /// String limpa, sem criptografia a ser validada.
        /// </summary>
        [DataMember]
        public string RawData { get; set; }

        /// <summary>
        /// String criptografada com HMAC SHA512.
        /// </summary>
        [DataMember]
        public string EncryptedData { get; set; }
    }
}
