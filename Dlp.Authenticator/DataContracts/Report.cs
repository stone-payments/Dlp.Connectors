
namespace Dlp.Authenticator.DataContracts {

    /// <summary>
    /// Report containing any information about an unexpected behavior occurred in the called service.
    /// </summary>
    public sealed class Report {

        /// <summary>
        /// Instantiate the report class.
        /// </summary>
        public Report() { }

        /// <summary>
        /// Gets the name of the field related to the report.
        /// </summary>
        public string Field { get; set; }

        /// <summary>
        /// GHets the message describing the situation about the field.
        /// </summary>
        public string Message { get; set; }
    }
}
