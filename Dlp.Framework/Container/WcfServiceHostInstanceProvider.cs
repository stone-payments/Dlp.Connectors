using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Framework.Container {

    public sealed class WcfServiceHostInstanceProvider : IInstanceProvider {

        private readonly Type _serviceType;

        public WcfServiceHostInstanceProvider(Type serviceType) {

            this._serviceType = serviceType;
        }

        public object GetInstance(InstanceContext instanceContext, Message message) {

            object instance = IocFactory.Resolve(this._serviceType);

            return instance;
        }

        public object GetInstance(InstanceContext instanceContext) {

            return GetInstance(instanceContext, null);
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance) {
            
        }
    }
}
