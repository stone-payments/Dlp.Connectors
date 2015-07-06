using Dlp.Framework.Container.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Framework.Container {

    public sealed class ComponentInfo<TInterface> : AbstractComponentInfo, IRegistration where TInterface : class {

        public ComponentInfo() { }

        /// <summary>
        /// Gets the last registered component data.
        /// </summary>
        private ComponentData LastRegisteredComponentData { get; set; }

        /// <summary>
        /// Associates a concrete class with the specified interface.
        /// </summary>
        /// <typeparam name="TConcrete">Type of the concrete class to be associated.</typeparam>
        /// <returns>Returns an instance of ComponentInfo to be registered with the IocFactory.Register method.</returns>
        public ComponentInfo<TInterface> ImplementedBy<TConcrete>(string name = null) where TConcrete : TInterface {

            // Verifica se a interface esta definda.
            if (this.InterfaceType == null) { throw new InvalidOperationException("The InterfaceType property cannot be null."); }

            // Obtém o tipo concreto a ser registrado.
            Type concreteType = typeof(TConcrete);

            // Caso o tipo ainda não esteja registrado, adiciona na lista de tipos concretos.
            if (this.ActualComponentDataCollection.Any(p => p.ConcreteType == concreteType) == false) {

                ComponentData componentData = new ComponentData();
                componentData.ConcreteType = concreteType;
                componentData.Name = name ?? concreteType.FullName;

                this.ActualComponentDataCollection.Add(componentData);

                // Armazena o último componente registrado.
                this.LastRegisteredComponentData = componentData;
            }

            return this;
        }

        /// <summary>
        /// Defines the last registered concrete type as a singleton.
        /// </summary>
        /// <returns>Returns an instance of ComponentInfo to be registered with the IocFactory.Register method.</returns>
        public ComponentInfo<TInterface> IsSingleton() {

            // Sai do método caso não existam typos definidos.
            if (this.ActualComponentDataCollection.Any() == false) { return this; }

            // Define o nome a ser utilizado ao registrar o componente.
            this.LastRegisteredComponentData.IsSingleton = true;

            return this;
        }

        /// <summary>
        /// Defines the last registered concrete type as the default type to be instantiated, when multiple types are associated to the same interface.
        /// </summary>
        /// <returns>Returns an instance of ComponentInfo to be registered with the IocFactory.Register method.</returns>
        public ComponentInfo<TInterface> IsDefault() {

            // Sai do método caso não existam typos definidos.
            if (this.ActualComponentDataCollection.Any() == false) { return this; }

            // Define o nome a ser utilizado ao registrar o componente.
            this.LastRegisteredComponentData.IsDefault = true;

            return this;
        }

        /// <summary>
        /// Informs that any type or property contained within this type should be resolved automatically.
        /// </summary>
        /// <returns>Returns an instance of ComponentInfo to be registered with the IocFactory.Register method.</returns>
        public ComponentInfo<TInterface> ResolveDependencies() {

            // Verifica se a interface esta definda.
            if (this.InterfaceType == null) { throw new InvalidOperationException("The InterfaceType property cannot be null."); }

            base.ResolveDependencies = true;

            return this;
        }

        /// <summary>
        /// Associates an interceptor to be executed when any method of the interface is called.
        /// To create an interceptor, a class must implements the IInterceptor interface.
        /// </summary>
        /// <typeparam name="TInterceptor">Type of the class that is going to intercept the requests.</typeparam>
        /// <returns>Returns an instance of ComponentInfo to be registered with the IocFactory.Register method.</returns>
        public ComponentInfo<TInterface> Interceptor<TInterceptor>() where TInterceptor : IInterceptor {

            Type interceptorType = typeof(TInterceptor);

            // Caso o tipo ainda não esteja registrado, adiciona na lista de tipos concretos.
            if (this.ActualInterceptorCollection.Contains(interceptorType) == false) {

                this.ActualInterceptorCollection.Add(interceptorType);
            }

            return this;
        }

        public ComponentInfo<TInterface> Instance(object instance) {

            // Verifica se a interface esta definda.
            if (this.InterfaceType == null) { throw new InvalidOperationException("The InterfaceType property cannot be null."); }

            // Obtém o tipo concreto a ser registrado.
            Type concreteType = instance.GetType();

            // Caso o tipo ainda não esteja registrado, adiciona na lista de tipos concretos.
            if (this.ActualComponentDataCollection.Any(p => p.ConcreteType == concreteType) == false) {

                ComponentData componentData = new ComponentData();
                componentData.ConcreteType = concreteType;
                componentData.Name = concreteType.FullName;
                componentData.Instance = instance;

                this.ActualComponentDataCollection.Add(componentData);

                // Armazena o último componente registrado.
                this.LastRegisteredComponentData = componentData;
            }

            return this;
        }

        public IRegistrationInfo[] Register() {

            return new IRegistrationInfo[] { this };
        }
    }
}
