using System;

namespace Dlp.Authenticator.Utility {

    /// <summary>
    /// Interface representing the configuration data.
    /// </summary>
    public interface IConfigurationUtility {

        /// <summary>
        /// Gets the GlobalIdentity service host address.
        /// </summary>
        string GlobalIdentityHostAddress { get; }
    }
}
