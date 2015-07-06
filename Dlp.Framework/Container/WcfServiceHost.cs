using System;
using System.ServiceModel;

namespace Dlp.Framework.Container {

    public sealed class WcfServiceHost : ServiceHost {

        public WcfServiceHost() { }

        public WcfServiceHost(Type serviceType, params Uri[] baseAddresses)
            : base(serviceType, baseAddresses) { }

        protected override void OnOpening() {
            
            this.Description.Behaviors.Add(new WcfServiceFactoryBehavior());

            base.OnOpening();
        }

        protected override void OnClosing() {
            base.OnClosing();
        }

        protected override void ApplyConfiguration() {

            // First, we call base.ApplyConfiguration() to read any configuration that was provided for
            // the service we're hosting. After this call, this.Description describes the service as it was configured.
            base.ApplyConfiguration();

            // Implementar configurações adicionais abaixo desta linha.
        }
    }
}
