using Dlp.Authenticator.Utility;
using Dlp.Framework;
using Dlp.Framework.Container;

namespace Dlp.Authenticator {

    /// <summary>
    /// Base class representing an authentication service.
    /// </summary>
    public abstract class AbstractAuthenticator {

        /// <summary>
        /// Initializes the authentication service.
        /// </summary>
        protected AbstractAuthenticator() {

            IocFactory.Register(
                Component.For<IConfigurationUtility>()
                .ImplementedBy<ConfigurationUtility>()
                .IsSingleton()
                );
        }

        private IConfigurationUtility _configurationUtility;
        /// <summary>
        /// Obtém uma instancia do utilitário de acesso ao arquivo de configuração.
        /// </summary>
        protected IConfigurationUtility ConfigurationUtility {
            get {
                if (this._configurationUtility == null) { this._configurationUtility = IocFactory.Resolve<IConfigurationUtility>(); }
                return this._configurationUtility;
            }
        }
    }
}
