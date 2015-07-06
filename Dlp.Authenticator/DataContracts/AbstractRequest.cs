using System;

namespace Dlp.Authenticator.DataContracts {

    /// <summary>
    /// Base class representing any request received by the Dlp.Authenticator.
    /// </summary>
    public abstract class AbstractRequest {

        /// <summary>
        /// Initializes the AbstractRequest base class.
        /// </summary>
        public AbstractRequest() { }

        /// <summary>
        /// Gets the application key that is calling the API.
        /// </summary>
        public Guid ApplicationKey { get; set; }
    }
}
