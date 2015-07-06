using Dlp.Framework.Container.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Framework.Container {

    public abstract class AbstractComponentInfo : IRegistrationInfo {

        /// <summary>
        /// Gets the interface type to be registered.
        /// </summary>
        public Type InterfaceType { get; internal set; }

        /// <summary>
        /// Gets all the interceptors associated with the interface to be registered.
        /// </summary>
        public IEnumerable<Type> InterceptorCollection { get { return this.ActualInterceptorCollection.ToList(); } }

        /// <summary>
        /// Gets all the implemented types for the interface to be registered.
        /// </summary>
        public IEnumerable<ComponentData> ComponentDataCollection { get { return this.ActualComponentDataCollection.ToList(); } }

        /// <summary>
        /// Gets or sets the flag defining whether the properties of this type that are registeres should be automatically resolved.
        /// </summary>
        public bool ResolveDependencies { get; set; }

        private List<Type> _actualInterceptorCollection;
        /// <summary>
        /// Gets the interceptors registered for the interface type.
        /// </summary>
        protected List<Type> ActualInterceptorCollection {
            get {
                if (this._actualInterceptorCollection == null) { this._actualInterceptorCollection = new List<Type>(); }
                return this._actualInterceptorCollection;
            }
        }

        private List<ComponentData> _actualComponentDataCollection;
        /// <summary>
        /// Gets the collection of components that implements the interface.
        /// </summary>
        protected List<ComponentData> ActualComponentDataCollection {
            get {
                if (this._actualComponentDataCollection == null) { this._actualComponentDataCollection = new List<ComponentData>(); }
                return this._actualComponentDataCollection;
            }
        }

        internal void AddInterceptor<TInterceptor>() where TInterceptor : IInterceptor, new() {

            this.ActualInterceptorCollection.Add(typeof(TInterceptor));
        }

        internal void AddInterceptor(Type interceptorType) {

            this.ActualInterceptorCollection.Add(interceptorType);
        }

        internal void AddComponentData(ComponentData componentData) {

            if (componentData == null) { return; }

            this.ActualComponentDataCollection.Add(componentData);
        }
    }
}
