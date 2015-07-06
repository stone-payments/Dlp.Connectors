
namespace Dlp.Authenticator.DataContracts {

    /// <summary>
    /// Class containing the result of a token validation process.
    /// </summary>
    public sealed class ValidateTokenResponse : AbstractResponse {

        /// <summary>
        /// Instantiates the ValidateTokenResponse class.
        /// </summary>
        public ValidateTokenResponse() : base() { }

        /// <summary>
        /// Gets the token expiration in minutes.
        /// </summary>
        public int ExpirationInMinutes { get; set; }

        /// <summary>
        /// Gets the email of the user associated with the validated token.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets the name of the user associated with the validated token.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the name of the application that first authenticated the user.
        /// </summary>
        public string SourceApplication { get; set; }

    }
}
