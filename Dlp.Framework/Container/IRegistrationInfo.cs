using System;
using System.Collections.Generic;

namespace Dlp.Framework.Container {
    
    public interface IRegistrationInfo {

        /// <summary>
        /// Gets the registered interface type.
        /// </summary>
        Type InterfaceType { get; }

        /// <summary>
        /// Gets all the interceptors for the interface to be registered.
        /// </summary>
        IEnumerable<Type> InterceptorCollection { get; }

        /// <summary>
        /// Gets all the components data for the interface to be registered.
        /// </summary>
        IEnumerable<ComponentData> ComponentDataCollection { get; }

        /// <summary>
        /// Gets or sets the flag that informs whether any type or property contained within this type should be resolved automatically.
        /// </summary>
        bool ResolveDependencies { get; }
    }
}
