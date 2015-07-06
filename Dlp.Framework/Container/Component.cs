using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Framework.Container {

    /// <summary>
    /// Represents a component to be registered within the container.
    /// </summary>
    public sealed class Component {

        /// <summary>
        /// Prepares the interface to be registered. Use this method to register types at the IocFactory.Register method.
        /// </summary>
        /// <typeparam name="TInterface">Type of the interface to be registered.</typeparam>
        /// <returns>Returns an instance of ComponentInfo to be registered with the IocFactory.Register method.</returns>
        public static ComponentInfo<TInterface> For<TInterface>() where TInterface : class {

            // Cria uma instancia do RegistrationInfo.
            ComponentInfo<TInterface> registrationInfo = new ComponentInfo<TInterface>();

            // Define a interface a ser registrada.
            registrationInfo.InterfaceType = typeof(TInterface);

            // Retorna o RegistrationInfo a ser registrado.
            return registrationInfo;
        }

        /// <summary>
        /// Load all the types from the specified namespace for the assembly that is calling this method, to be registered at the IocFactory.Register method.
        /// </summary>
        /// <param name="namespace">Namespace containing the types to be registered.</param>
        /// <returns>Returns an instance of AssemblyInfo to be registered with the IocFactory.Register method.</returns>
        public static AssemblyInfo FromThisAssembly(string @namespace) {

            // Obtém o assembly que esta chamando o método.
            Assembly assembly = Assembly.GetCallingAssembly();

            // Prepara as classes a serem registradas.
            return Component.FromAssembly(assembly, @namespace);
        }

        /// <summary>
        /// Load all the types from the specified namespace for the specified assembly, to be registered at the IocFactory.Register method.
        /// </summary>
        /// <param name="assembly">Assembly containing the types to be registered.</param>
        /// <param name="namespace">Namespace containing the types to be registered.</param>
        /// <returns>Returns an instance of AssemblyInfo to be registered with the IocFactory.Register method.</returns>
        public static AssemblyInfo FromAssembly(Assembly assembly, string @namespace) {

            // Obtém todas as classes do namespace especificado.
            IEnumerable<Type> typeCollection = Component.LoadAssemblyNamespaceTypes(assembly, @namespace);

            AssemblyInfo assemblyInfo = new AssemblyInfo();

            // Registra cada uma das classes encontradas no namespace.
            foreach (Type type in typeCollection) {

                // Obtém todas as interfaces associadas com o tipo concreto.
                IEnumerable<Type> interfaceTypeCollection = type.GetInterfaces().Where(p => p.FullName.StartsWith("System.") == false);

                // Registra cada uma das interfaces encontradas.
                foreach (Type interfaceType in interfaceTypeCollection) {

                    // Obtém o elemento que representa a interface a ser registrada.
                    ComponentInfo componentInfo = assemblyInfo.ComponentInfoCollection.FirstOrDefault(p => p.InterfaceType == interfaceType);

                    // Caso o elemento não tenha sido encontrado, cria um novo.
                    if (componentInfo == null) {
                    
                        componentInfo = new ComponentInfo();
                        componentInfo.InterfaceType = interfaceType;

                        assemblyInfo.ComponentInfoCollection.Add(componentInfo);
                    }

                    // Caso o tipo concreto ainda não esteja associado, cria a associação.
                    if (componentInfo.ComponentDataCollection.Any(p => p.ConcreteType == type) == false) {

                        ComponentData componentData = new ComponentData();

                        componentData.ConcreteType = type;
                        componentData.Name = type.FullName;

                        componentInfo.AddComponentData(componentData);
                    }
                }
            }

            return assemblyInfo;
        }

        private static IEnumerable<Type> LoadAssemblyNamespaceTypes(Assembly assembly, string @namespace) {

            // Retorna nulo, caso não tenha sido especificado um assembly.
            if (assembly == null) { return null; }

            IEnumerable<Type> loadedTypes = null;

            // Carrega todos os tipos definidos no assembly.
            try {
                loadedTypes = assembly.GetTypes();
            }
            // Caso algum tipo do assembly referenciado não esteja presente no sistema, obtém apenas os tipos válidos.
            catch (ReflectionTypeLoadException ex) {
                loadedTypes = ex.Types.Where(p => p != null);
            }

            // Retorna todos os tipos concretos do namespace especificado.
            return loadedTypes.Where(p =>
                    @namespace.Equals(p.Namespace, StringComparison.InvariantCultureIgnoreCase)
                    && p.IsClass == true && p.IsAbstract == false);
        }
    }
}
