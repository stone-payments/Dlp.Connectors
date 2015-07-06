using System.Collections.Generic;

namespace Dlp.Authenticator.DataContracts {

    /// <summary>
    /// Base class representing any response returned by the Dlp.Authenticator.
    /// </summary>
    public abstract class AbstractResponse {

        /// <summary>
        /// Initializes the AbstractResponse base class.
        /// </summary>
        public AbstractResponse() {
            this.OperationReport = new List<Report>();
        }

        /// <summary>
        /// Gets the flag that indicates if the operation was successfully accomplished.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets any information about errors and unexpected behaviors occurred in the request.
        /// </summary>
        public List<Report> OperationReport { get; set; }
    }
}
