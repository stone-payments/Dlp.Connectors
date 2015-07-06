using System;

namespace Dlp.Authenticator.DataContracts {

    /// <summary>
    /// Class containing the data of the client application to be validated.
    /// </summary>
    public sealed class ValidateApplicationRequest : AbstractRequest {

        /// <summary>
        /// Instantiates the ValidateApplicationRequest class.
        /// </summary>
        public ValidateApplicationRequest() : base() { }

        /// <summary>
        /// Gets or sets the client application key to be validated.
        /// </summary>
        public Guid ClientApplicationKey { get; set; }

        /// <summary>
        /// Gets or sets the encrypted data sent by the client.
        /// </summary>
        public string EncryptedData { get; set; }

        /// <summary>
        /// Gets or sets the raw data sent by the client.
        /// </summary>
        public string RawData { get; set; }
    }
}
