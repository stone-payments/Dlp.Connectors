using System;

namespace Dlp.Authenticator.DataContracts {

    /// <summary>
    /// Class containing the data of the user to be authenticated.
    /// </summary>
    public sealed class AuthenticateUserRequest : AbstractRequest {

        /// <summary>
        /// Instantiates the AuthenticateRequest class.
        /// </summary>
        public AuthenticateUserRequest() { }

        /// <summary>
        /// Gets or sets the user email (login) to be validated.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Gets or sets the user password.
        /// </summary>
        public string Password { get; set; }
    }
}
