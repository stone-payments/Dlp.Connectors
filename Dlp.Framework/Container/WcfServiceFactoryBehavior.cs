using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Framework.Container {

    public sealed class WcfServiceFactoryBehavior : IServiceBehavior {

        public WcfServiceFactoryBehavior() { }

        public void AddBindingParameters(ServiceDescription serviceDescription, ServiceHostBase serviceHostBase, Collection<ServiceEndpoint> endpoints, BindingParameterCollection bindingParameters) { }

        public void ApplyDispatchBehavior(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase) {

            foreach (ChannelDispatcherBase channelDispatcherBase in serviceHostBase.ChannelDispatchers) {

                ChannelDispatcher channelDispatcher = channelDispatcherBase as ChannelDispatcher;

                if (channelDispatcher != null) {

                    foreach (EndpointDispatcher endpointDispatcher in channelDispatcher.Endpoints) {

                        Type interfaceType = serviceDescription.ServiceType.GetInterfaces()[0];

                        if (interfaceType == null) { throw new InvalidOperationException(string.Format("Tipo: {0}", serviceDescription.ServiceType.Name)); }

                        endpointDispatcher.DispatchRuntime.InstanceProvider = new WcfServiceHostInstanceProvider(interfaceType);
                    }
                }
            }
        }

        public void Validate(ServiceDescription serviceDescription, System.ServiceModel.ServiceHostBase serviceHostBase) { }
    }
}
