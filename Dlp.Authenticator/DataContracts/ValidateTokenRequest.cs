namespace Dlp.Authenticator.DataContracts {

    /// <summary>
    /// Class containing the data to validated.
    /// </summary>
    public sealed class ValidateTokenRequest : AbstractRequest {

        /// <summary>
        /// Instantiates the ValidateTokenRequest class.
        /// </summary>
        public ValidateTokenRequest() { }

        /// <summary>
        /// gets or sets the user authentication token to be validated.
        /// </summary>
        public string Token { get; set; }
    }
}
