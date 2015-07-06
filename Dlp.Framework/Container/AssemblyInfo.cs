using Dlp.Framework.Container.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Framework.Container {

    public sealed class AssemblyInfo : IRegistration {

        public AssemblyInfo() { }

        private List<ComponentInfo> _componentInfoCollection;
        public List<ComponentInfo> ComponentInfoCollection {
            get {
                if (this._componentInfoCollection == null) { this._componentInfoCollection = new List<ComponentInfo>(); }
                return this._componentInfoCollection;
            }
        }

        /// <summary>
        /// Associates an interceptor to be executed when any method of the interface is called.
        /// To create an interceptor, a class must implements the IInterceptor interface.
        /// </summary>
        /// <typeparam name="TInterceptor">Type of the class that is going to intercept the requests.</typeparam>
        /// <returns>Returns an instance of RegistrationInfo to be registered with the IocFactory.</returns>
        public AssemblyInfo Interceptor<TInterceptor>() where TInterceptor : IInterceptor, new() {

            Type interceptorType = typeof(TInterceptor);

            // Caso o tipo ainda não esteja registrado, adiciona na lista de tipos concretos.
            foreach (ComponentInfo componentInfo in this.ComponentInfoCollection.Where(p => p.InterceptorCollection.Contains(interceptorType) == false)) {

                componentInfo.AddInterceptor<TInterceptor>();
            }

            return this;
        }

        public AssemblyInfo IsSingleton<TConcrete>() {

            foreach (ComponentData componentData in this.ComponentInfoCollection.SelectMany(p => p.ComponentDataCollection).Where(q => q.ConcreteType == typeof(TConcrete))) {

                componentData.IsSingleton = true;
            }

            return this;
        }

        public IRegistrationInfo[] Register() {

            return this.ComponentInfoCollection.ToArray();
        }
    }
}
