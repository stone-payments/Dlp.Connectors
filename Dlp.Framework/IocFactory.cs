using Dlp.Framework.Container;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Dlp.Framework {
    /*
    /// <summary>
    /// Class representing on object registered by the IocFactory.
    /// </summary>
    internal sealed class IocData {

        public IocData() {

            this.ConcreteTypeDictionary = new Dictionary<string, Type>();
        }

        /// <summary>
        /// Tipo da interface que é assinada pelas classes concretas relacionadas.
        /// </summary>
        public Type DependencyInterface { get; set; }

        /// <summary>
        /// Lista de classes concretas que assinam a interface.
        /// </summary>
        public Dictionary<string, Type> ConcreteTypeDictionary { get; set; }
    }

    /// <summary>
    /// Dependency Injection Container.
    /// </summary>
    [Obsolete("Use the IocFactory from the Dlp.Framework.Container namespace.")]
    public static class IocFactory {

        private static List<IocData> _dependencyCollection;
        private static List<IocData> DependencyCollection {

            get {
                if (_dependencyCollection == null) { _dependencyCollection = new List<IocData>(); }
                return _dependencyCollection;
            }
            set { _dependencyCollection = value; }
        }

        /// <summary>
        /// Register a class to an interface.
        /// </summary>
        /// <typeparam name="T1">Interface to be registered.</typeparam>
        /// <typeparam name="T2">Concrete class to be used when instantiating the interface.</typeparam>
        public static void Register<T1, T2>() where T1 : class {

            // Registra a dependencia, informando um nome.
            Register<T1, T2>(typeof(T2).FullName);
        }

        /// <summary>
        /// Register a class to an interface.
        /// </summary>
        /// <param name="name">Dependency name. If two or more classes are registered with the same interface type, you can refer to the name to distinguish between them.</param>
        /// <typeparam name="T1">Interface to be registered.</typeparam>
        /// <typeparam name="T2">Concrete class to be used when instantiating the interface.</typeparam>
        public static void Register<T1, T2>(string name) where T1 : class {

            Type dependencyInterface = typeof(T1);
            Type dependency = typeof(T2);

            // Verifica se os tipos foram especificados.
            if (dependencyInterface == null || dependency == null) {
                throw new InvalidOperationException("The dependency and dependencyInterface cannot be null.");
            }

            // Verifica se o tipo concreto implementa a interface.
            if (dependencyInterface.IsAssignableFrom(dependency) == false) {
                throw new InvalidOperationException("The dependency must implement the specified interface.");
            }

            // Verifica se o nome de identificação da dependencia foi informado.
            if (string.IsNullOrWhiteSpace(name) == true) {
                throw new ArgumentNullException("name");
            }

            // Registra a interface com o tipo concreto.
            Register(dependencyInterface, dependency, dependency.FullName);
        }

        /// <summary>
        /// Register an interface to an concrete class.
        /// </summary>
        /// <param name="dependencyInterface"></param>
        /// <param name="dependency"></param>
        /// <param name="name"></param>
        private static void Register(Type dependencyInterface, Type dependency, string name = null) {

            // Verifica se já existe alguma dependência para a interface especificada.
            IocData iocData = DependencyCollection.FirstOrDefault(p => p.DependencyInterface == dependencyInterface);

            // Verifica se a interface já esta registrada. Caso positivo, atualiza a concreta.
            if (iocData != null) {

                // Tenta encontrar o tipo especificado na coleção de tipos.
                string typeName = iocData.ConcreteTypeDictionary.Keys.FirstOrDefault(p => p.Equals(name, StringComparison.InvariantCultureIgnoreCase));

                // Caso o tipo não esteja registrado, adiciona.
                if (string.IsNullOrWhiteSpace(typeName) == true) {

                    // Adiciona a classe concreta associada a interface.
                    iocData.ConcreteTypeDictionary.Add(name, dependency);
                }
                else {
                    // Caso contrário, atualiza. 
                    iocData.ConcreteTypeDictionary[typeName] = dependency;
                }
            }
            else {
                iocData = new IocData();

                iocData.DependencyInterface = dependencyInterface;
                iocData.ConcreteTypeDictionary.Add(name, dependency);

                // Adiciona um novo registro para a interface.
                DependencyCollection.Add(iocData);
            }
        }

        /// <summary>
        /// Register all the classes from a namespace.
        /// </summary>
        /// <param name="qualifiedNamespace">Namespace containing the classes to register.</param>
        public static void RegisterNamespace(string qualifiedNamespace) {

            // Obtém todas as classes do namespace especificado.
            IEnumerable<Type> typeCollection = Assembly.GetCallingAssembly().LoadAssemblyNamespaceTypes(qualifiedNamespace);

            // Registra cada uma das classes encontradas no namespace.
            foreach (Type type in typeCollection) {

                // Obtém todas as interfaces associadas com o tipo concreto.
                IEnumerable<Type> interfaceTypeCollection = type.GetInterfaces().Where(p => p.FullName.StartsWith("System.") == false);

                // Registra cada uma das interfaces encontradas.
                foreach (Type interfaceType in interfaceTypeCollection) {

                    // Registra a interface com o tipo concreto.
                    Register(interfaceType, type, type.FullName);
                }
            }
        }

        private static IEnumerable<Type> LoadAssemblyNamespaceTypes(this Assembly assembly, string qualifiedNamespace) {

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
                    qualifiedNamespace.Equals(p.Namespace, StringComparison.InvariantCultureIgnoreCase)
                    && p.IsClass == true && p.IsAbstract == false);
        }

        /// <summary>
        /// Given the dependency name, gets a new instance of the type registered to the specified interface.
        /// </summary>
        /// <typeparam name="T">Interface type previously registered.</typeparam>
        /// <param name="name">Name of the desired dependency.</param>
        /// <param name="constructorParameters">Constructor parameters for the concrete type.</param>
        /// <returns>Returns a new instance for the concrete type registered with the T interface.</returns>
        public static T ResolveSpecific<T>(string name, params object[] constructorParameters) where T : class {

            // Obtém o tipo da interface solicitada.
            Type interfaceType = typeof(T);

            // Tenta obter as dependencias para interface especificada.
            IocData iocData = DependencyCollection.FirstOrDefault(p => p.DependencyInterface == interfaceType);

            // Verifica se existe uma mapeamento para a interface.
            if (iocData == null) { throw new InvalidOperationException(string.Format("The interface type {0} is not registered.", interfaceType.FullName)); }

            // Verifica se existe alguma dependencia cadastrada com o nome especificado.
            if (iocData.ConcreteTypeDictionary.ContainsKey(name) == false) {

                // Dispara a exceção informando que o tipo concreto não esta registrado com a interface selecionada.
                throw new InvalidOperationException(string.Format("The name {0} is not registered with interface {1}.", name, interfaceType.FullName));
            }

            // Obtém o tipo concreto a ser instanciado.
            Type actualConcreteType = iocData.ConcreteTypeDictionary[name];

            // Verifica se foram especificados os parâmetros do construtor.
            if (actualConcreteType == null) {

                // Dispara a exceção informando que o tipo concreto não esta registrado com a interface selecionada.
                throw new InvalidOperationException(string.Format("The concrete type {0} is not registered with interface {1}.", actualConcreteType.FullName, interfaceType.FullName));
            }

            // Cria e retorna uma nova instancia do tipo concreto com os parâmetros especificados para o contrutor.
            return Activator.CreateInstance(actualConcreteType, constructorParameters) as T;
        }

        /// <summary>
        /// Gets a new instance of the type registered to the specified interface.
        /// </summary>
        /// <typeparam name="T">Interface type previously registered.</typeparam>
        /// <param name="constructorParameters">Constructor parameters for the concrete type.</param>
        /// <returns>Returns a new instance for the concrete type registered with the T interface.</returns>
        public static T Resolve<T>(params object[] constructorParameters) where T : class {

            // Obtém o tipo da interface solicitada.
            Type interfaceType = typeof(T);

            // Obtém a informação do registro de dependencias.
            IocData iocData = DependencyCollection.FirstOrDefault(p => p.DependencyInterface == interfaceType);

            // Verifica se existe uma mapeamento para a interface.
            if (iocData == null) { throw new ArgumentException(string.Format("The interface type {0} is not registered.", interfaceType.FullName)); }

            // Obtém o primeiro tipo concreto registrado, que será instanciado.
            Type concreteType = iocData.ConcreteTypeDictionary.Values.FirstOrDefault();

            // Verifica se foram especificados os parâmetros do construtor.
            if (constructorParameters.Any() == true) {

                // Cria e retorna uma nova instancia do tipo concreto com os parâmetros especificados para o contrutor.
                return Activator.CreateInstance(concreteType, constructorParameters) as T;
            }

            // Cria a instancia do objeto selecionado.
            return CreateInstance(interfaceType, constructorParameters) as T;
        }

        /// <summary>
        /// Checks if an interface is already registered.
        /// </summary>
        /// <param name="interfaceType">Type of the interface to be checked.</param>
        /// <returns>Returns true if the interface is already registered.</returns>
        public static bool IsRegistered(Type interfaceType) {

            // Retorna true caso a interface já esteja registrada.
            return DependencyCollection.Any(p => p.DependencyInterface == interfaceType);
        }

        /// <summary>
        /// Gets all the concrete types registered with the specified interface type.
        /// </summary>
        /// <param name="interfaceType">Type of the interface to be checked.</param>
        /// <returns>Returns a collection of types registered with the specified interface.</returns>
        public static IEnumerable<Type> RegisteredTypesForInterface(Type interfaceType) {

            List<Type> registeredTypeCollection = new List<Type>();

            // Obtém a informação do registro de dependencias.
            IocData iocData = DependencyCollection.FirstOrDefault(p => p.DependencyInterface == interfaceType);

            // Verifica se existe uma mapeamento para a interface.
            if (iocData != null) {

                // Retorna todos os tipos associados a interface.
                registeredTypeCollection = iocData.ConcreteTypeDictionary.Select(p => p.Value).ToList();
            }

            return registeredTypeCollection;
        }

        private static object CreateInstance(Type objectType, params object[] constructorParameters) {

            // Obtém a informação do registro de dependencias.
            IocData iocData = DependencyCollection.FirstOrDefault(p => p.DependencyInterface == objectType);

            // Verifica se existe uma mapeamento para a interface.
            if (iocData == null) { return null; }

            // Obtém o primeiro tipo concreto registrado para ser instanciado.
            Type concreteType = iocData.ConcreteTypeDictionary.Values.FirstOrDefault();

            // Obtém os construtores públicos da classe concreta.
            ConstructorInfo[] constructorInfoCollection = concreteType.GetConstructors();

            bool hasDefaultContructor = false;

            // Analisa todos os construtores disponíveis.
            foreach (ConstructorInfo constructorInfo in constructorInfoCollection) {

                // Obtém os parâmetros do construtor.
                ParameterInfo[] parameterInfoCollection = constructorInfo.GetParameters();

                // Verifica se foi encontrado um construtor sem parâmetros.
                if (parameterInfoCollection.Length == 0) { hasDefaultContructor = true; }
                else {

                    IList<object> parameters = new List<object>();

                    // Verifica cada parâmetro do construtor.
                    foreach (ParameterInfo parameterInfo in parameterInfoCollection) {

                        Type parameterType = parameterInfo.ParameterType;

                        // Verifica se não é um tipo base do .NET.
                        if (parameterType.FullName.StartsWith("System.") == false) {

                            // Tenta localizar alguma classe registrada que seja do mesmo tipo do construtor.
                            Type dependencyType = DependencyCollection.Select(p => p.DependencyInterface).FirstOrDefault(p => p.IsAssignableFrom(parameterType) == true);

                            // Caso nenhum tipo tenha sido encontrado, passa para o próximo parâmetro.
                            if (dependencyType == null) { continue; }

                            // Adiciona uma instancia do tipo encontrado na lista de parâmetros do construtor.
                            parameters.Add(CreateInstance(parameterType));
                        }
                    }

                    // Verifica se a quantidade de parâmetros esta de acordo com a quantidade de objetos disponíveis.
                    if (parameters.Count == parameterInfoCollection.Length) { return Activator.CreateInstance(concreteType, parameters.ToArray()); }
                }
            }

            // Verifica se foi especificado algum parâmetro para o contrutor.
            if (constructorParameters.Length == 0 && hasDefaultContructor == true) {
                // Cria e retorna uma nova instancia do tipo concreto com o construtor padrão.
                return Activator.CreateInstance(concreteType);
            }
            else {
                // Cria e retorna uma nova instancia do tipo concreto com os parâmetros especificados para o contrutor.
                return Activator.CreateInstance(concreteType, constructorParameters);
            }
        }
    }
    */
}