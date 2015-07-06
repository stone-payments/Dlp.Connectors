using System;
using System.ServiceModel;
using System.ServiceModel.Activation;

namespace Dlp.Framework.Container {

    public abstract class WcfServiceHostFactory : ServiceHostFactory {

        public WcfServiceHostFactory() { }

        protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses) {

            // Registra os componentes a serem utilizados na aplicação.
            this.InitializeComponents();

            return new WcfServiceHost(serviceType, baseAddresses);
        }

        /// <summary>
        /// Initializes and register the components within the IocFactory inside this method.
        /// </summary>
        protected abstract void InitializeComponents();
    }
}
