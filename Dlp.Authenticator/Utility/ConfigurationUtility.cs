using System.Configuration;

namespace Dlp.Authenticator.Utility {

    internal sealed class ConfigurationUtility : IConfigurationUtility {

        public ConfigurationUtility() { }

        /// <summary>
        /// Obtém o endereço do servidor GlobalIdentity.
        /// </summary>
        public string GlobalIdentityHostAddress { get { return ConfigurationManager.AppSettings["GlobalIdentityHostAddress"]; } }
    }
}
