using Dlp.Framework.Container.Interceptors;
using Dlp.Framework.Container.Proxies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Framework.Container {

	/// <summary>
	/// Dependency Injection Container.
	/// </summary>
	public static class IocFactory {

		private static List<IRegistrationInfo> _registrationInfoCollection;
		private static List<IRegistrationInfo> RegistrationInfoCollection {
			get {
				if (_registrationInfoCollection == null) { _registrationInfoCollection = new List<IRegistrationInfo>(); }
				return _registrationInfoCollection;
			}
		}

		private static Dictionary<string, object> _singletonInstanceDictionary;
		private static Dictionary<string, object> SingletonInstanceDictionary {
			get {
				if (_singletonInstanceDictionary == null) { _singletonInstanceDictionary = new Dictionary<string, object>(); }
				return _singletonInstanceDictionary;
			}
		}

		/// <summary>
		/// Register a component within the IocFactory.
		/// </summary>
		/// <param name="componentRegistrationCollection">The component to be registered. Create with the Component.For method.</param>
		/// <include file='Samples/IocFactory.xml' path='Docs/Members[@name="Register"]/*'/>
		public static void Register(params IRegistration[] componentRegistrationCollection) {

			foreach (IRegistration componentRegistration in componentRegistrationCollection) {

				// Obtém todos os componentes a serem registrados.
				IRegistrationInfo[] registrationInfoCollection = componentRegistration.Register();

				foreach (IRegistrationInfo registrationInfo in registrationInfoCollection) {

					// Tenta localizar um componente para a interface recebida.
					IRegistrationInfo registeredRegistrationInfo = RegistrationInfoCollection.FirstOrDefault(p => p.InterfaceType == registrationInfo.InterfaceType);

					// Caso não exista nenhum componente do mesmo tipo, adiciona-o a lista de componentes registrados.
					if (registeredRegistrationInfo == null) {
						RegistrationInfoCollection.Add(registrationInfo);
					}
					else {

						// Adiciona cada novo ComponentData.
						foreach (ComponentData componentData in registrationInfo.ComponentDataCollection) {

							// Verifica se o componente já existe para o tipo registrado. Caso negativo, adiciona o componente.
							if (registeredRegistrationInfo.ComponentDataCollection.Any(p => p.ConcreteType == componentData.ConcreteType) == false) {

								AbstractComponentInfo componentInfo = (AbstractComponentInfo)registeredRegistrationInfo;

								// Adiciona o componente ao tipo.
								componentInfo.AddComponentData(componentData);
							}
						}

						// Adiciona cada novo interceptor.
						foreach (Type type in registrationInfo.InterceptorCollection) {

							// Verifica se o interceptor já existe para o tipo registrado. Caso negativo, adiciona o interceptor.
							if (registeredRegistrationInfo.InterceptorCollection.Any(p => p == type) == false) {

								AbstractComponentInfo componentInfo = (AbstractComponentInfo)registeredRegistrationInfo;

								// Adiciona o interceptor.
								componentInfo.AddInterceptor(type);
							}
						}
					}
				}
			}

			// Adiciona os componentes na lista de items registrados.
			//RegistrationInfoCollection.AddRange(registrationInfoCollection);
		}

		/// <summary>
		/// Clears all the registered data and interceptors.
		/// </summary>
		public static void Reset() {

			SingletonInstanceDictionary.Clear();
			RegistrationInfoCollection.Clear();
		}

		internal static object Resolve(Type interfaceType, params object[] constuctorParameters) {

			return Resolve(interfaceType, null, constuctorParameters);
		}

		/// <summary>
		/// Gets the class registered with the provided interface. If multiple types are associated with the interface, the first registered type is returned.
		/// </summary>
		/// <typeparam name="TInterface">Interface type.</typeparam>
		/// <param name="constructorParameters">If the concrete class needs any parameter to be instantiated, specify them here.</param>
		/// <returns>Returns an instance for the specified interface type.</returns>
		/// <exception cref="InvalidOperationException">Either the specified interface is not registered or there aren't any concrete types associated with the interface.</exception>
		/// <include file='Samples/IocFactory.xml' path='Docs/Members[@name="Resolve"]/*'/>
		public static TInterface Resolve<TInterface>(params object[] constructorParameters) where TInterface : class {

			return Resolve<TInterface>(null, constructorParameters);
		}

		/// <summary>
		/// Gets the class registered with the provided interface, by its name.
		/// </summary>
		/// <typeparam name="TInterface">Interface type.</typeparam>
		/// <param name="name">Unique name of the type to be returned.</param>
		/// <param name="constructorParameters">If the concrete class needs any parameter to be instantiated, specify them here.</param>
		/// <returns>Returns an instance for the specified interface type.</returns>
		/// <exception cref="InvalidOperationException">Either the specified interface is not registered or there aren't any concrete types associated with the interface.</exception>
		public static TInterface ResolveByName<TInterface>(string name, params object[] constructorParameters) where TInterface : class {

			return Resolve<TInterface>(name, constructorParameters);
		}

		private static TInterface Resolve<TInterface>(string name, params object[] constructorParameters) where TInterface : class {

			// Obtém o tipo da interface solicitada.
			Type interfaceType = typeof(TInterface);

			return Resolve(interfaceType, name, constructorParameters) as TInterface;
		}

		private static object Resolve(Type interfaceType, string name, params object[] constructorParameters) {

			// Obtém a informação do registro de dependencias.
			IRegistrationInfo registrationInfo = RegistrationInfoCollection.FirstOrDefault(p => p.InterfaceType == interfaceType);

			// Verifica se existe uma mapeamento para a interface.
			if (registrationInfo == null) { throw new InvalidOperationException(string.Format("The interface type {0} is not registered.", interfaceType.FullName)); }

			// Obtém as informações do primeiro tipo concreto registrado, que será instanciado.
			ComponentData componentData = null;

			// Verifica se foi especificado o nome do tipo a ser retornado.
			if (string.IsNullOrWhiteSpace(name) == true) {
				componentData = registrationInfo.ComponentDataCollection.OrderByDescending(p => p.IsDefault == true).FirstOrDefault();
			}
			else {
				componentData = registrationInfo.ComponentDataCollection.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
			}

			// Verifica se alguma classe concreta foi encontrada para a interface.
			if (componentData == null) { throw new InvalidOperationException(string.Format("The interface type {0} does not have any associated concrete types.", interfaceType.FullName)); }

			// Verifica se o tipo solicitado deve ser tratado como singleton.
			if (componentData.IsSingleton == true) {

				// Verifica se o tipo existe no dicionário de objetos instanciados.
				if (SingletonInstanceDictionary.ContainsKey(componentData.Name) == true) {

					// Retorna o objeto já existente.
					return SingletonInstanceDictionary[componentData.Name];
				}
			}

			// Obtém a instancia previamente definida pelo.
			object returnInstance = componentData.Instance;

			// Caso não exista uma instancia pré-definida, cria uma nova.
			if (componentData.Instance == null) {

				// Cria uma nova instancia.
				//returnInstance = CreateInstance(interfaceType, componentData, registrationInfo.InterceptorCollection, constructorParameters);
				returnInstance = CreateInstance(interfaceType, componentData, registrationInfo, constructorParameters);

				// Verifica se as propriedades desta classe que estejam registradas no container de injeção de dependência, devem ser instanciadas.
				if (registrationInfo.ResolveDependencies == true) {

					// Obtém todas as propriedades que não sejam de sistema.
					IEnumerable<PropertyInfo> propertyInfoCollection = interfaceType.GetProperties().Where(p => p.PropertyType.FullName.Equals("System") == false && p.CanWrite == true);

					foreach (PropertyInfo propertyInfo in propertyInfoCollection) {

						Type propertyInterfaceType = (propertyInfo.PropertyType.IsInterface == true) ? propertyInfo.PropertyType : propertyInfo.PropertyType.GetInterface(string.Format("I{0}", propertyInfo.Name));

						// Verifica se existe um registro para a interface da propriedade.
						if (RegistrationInfoCollection.Any(p => p.InterfaceType == propertyInterfaceType) == false) { continue; }

						// Obtém uma instancia para o tipo da propriedade.
						object propertyInstance = Resolve(propertyInterfaceType);

						// Caso não tenha sido possível obter uma instancia, passa para a próxima propriedade.
						if (propertyInstance == null) { continue; }

						// Define o valor da propriedade na instancia a ser retornada.
						propertyInfo.SetValue(returnInstance, propertyInstance);
					}
				}
			}

			// Caso o tipo deva ser tratado como singleton, adiciona a instancia recém criada no dicionário de objetos instanciados.
			if (componentData.IsSingleton == true && SingletonInstanceDictionary.ContainsKey(componentData.Name) == false) {

				SingletonInstanceDictionary.Add(componentData.Name, returnInstance);
			}

			return returnInstance;
		}

		private static object CreateInstance<TInterface>(ComponentData componentData, IRegistrationInfo registrationInfo, params object[] constructorParameters) {

			return CreateInstance(typeof(TInterface), componentData, registrationInfo, constructorParameters);
		}

		//private static object CreateInstance(Type interfaceType, ComponentData componentData, IEnumerable<Type> interceptorCollection, params object[] constructorParameters) {
		private static object CreateInstance(Type interfaceType, ComponentData componentData, IRegistrationInfo registrationInfo, params object[] constructorParameters) {

			// Obtém o primeiro tipo concreto registrado para ser instanciado.
			Type concreteType = componentData.ConcreteType;

			// Obtém os construtores públicos da classe concreta.
			ConstructorInfo[] constructorInfoCollection = concreteType.GetConstructors();

			// Verifica se existe um construtor padrão (sem parâmetros).
			bool hasDefaultContructor = constructorInfoCollection.Any(p => p.GetParameters().Length == 0);

			List<object> interceptors = new List<object>();

			if (registrationInfo != null && registrationInfo.InterceptorCollection.Any() == true) {

				foreach (Type interceptor in registrationInfo.InterceptorCollection) {

					// Cria uma instancia do interceptor.
					object interceptorInstance = Activator.CreateInstance(interceptor);

					interceptors.Add(interceptorInstance);
				}
			}

			// Caso exista um construtor padrão e nenhum parâmetro tenha sido especificado, cria a instancia.
			if (hasDefaultContructor == true && constructorParameters.Length == 0) {

				// Cria e retorna uma nova instancia do tipo concreto com o construtor padrão.
				object instance = CreateInstance(registrationInfo, concreteType);

				// Caso algum interceptor tenha sido especificado, utiliza um proxy.
				return (registrationInfo.InterceptorCollection.Any() == false) ? instance : DynamicProxy.NewInstance(instance, interceptors.OfType<IInterceptor>().ToArray());
			}

			// Caso não exista um construtor padrão, ou algum parâmetro tenha sido especificado.
			if (constructorParameters.Length > 0) {

				// Armazena a lista de tipos dos parâmetros do construtor.
				IEnumerable<Type> constructorParameterTypeCollection = constructorParameters.Select(p => p.GetType());

				// Analisa todos os construtores disponíveis.
				foreach (ConstructorInfo constructorInfo in constructorInfoCollection) {

					// Obtém os parâmetros do construtor.
					ParameterInfo[] parameterInfoCollection = constructorInfo.GetParameters();

					// Caso o construtor não possua a mesma quantidade de parâmetros que foram informados, passa para o próximo construtor.
					if (constructorParameterTypeCollection.Count() != parameterInfoCollection.Count()) { continue; }

					// Obtém a lista de tipos dos parâmetros.
					IEnumerable<Type> parameterTypeCollection = parameterInfoCollection.Select(p => p.ParameterType);

					IList<Type> matchedParameterList = new List<Type>();

					int parameterCounter = 0;

					// Percorre a lista de parâmetros encontrados no construtor.
					foreach (Type parameterType in parameterTypeCollection) {

						// Percorre a lista de parâmetros informados para instanciar o tipo.
						do {

							Type constructorParameterType = constructorParameterTypeCollection.ElementAt(parameterCounter);

							parameterCounter++;

							if (constructorParameterType == parameterType || (parameterType.IsInterface && parameterType.IsAssignableFrom(constructorParameterType) == true) || constructorParameterType.BaseType == parameterType) {
								matchedParameterList.Add(constructorParameterType);
								break;
							}

						} while (parameterCounter < constructorParameterTypeCollection.Count());
					}

					// Caso as assinaturas dos parâmetros e do construtor sejam identicas, utiliza o construtor atual.
					if (matchedParameterList.Count == parameterTypeCollection.Count()) {

						object instance = CreateInstance(registrationInfo, concreteType, constructorParameters);

						// Caso algum interceptor tenha sido especificado, utiliza um proxy.
						return (registrationInfo.InterceptorCollection.Any() == false) ? instance : DynamicProxy.NewInstance(instance, interceptors.OfType<IInterceptor>().ToArray());
					}
				}
			}

			IList<object> parameters = new List<object>();

			// Analisa todos os construtores disponíveis.
			foreach (ConstructorInfo constructorInfo in constructorInfoCollection) {

				// Obtém os parâmetros do construtor.
				ParameterInfo[] parameterInfoCollection = constructorInfo.GetParameters();

				// Verifica cada parâmetro do construtor.
				foreach (ParameterInfo parameterInfo in parameterInfoCollection) {

					Type parameterType = parameterInfo.ParameterType;

					// Verifica se não é um tipo base do .NET.
					if (parameterType.FullName.StartsWith("System.") == false) {

						// Tenta localizar alguma interface registrada que seja do mesmo tipo do construtor.
						IRegistrationInfo parameterRegistrationInfo = RegistrationInfoCollection.FirstOrDefault(p => p.InterfaceType.IsAssignableFrom(parameterType) == true);

						// Caso nenhum tipo tenha sido encontrado, passa para o próximo parâmetro.
						if (parameterRegistrationInfo == null) { continue; }

						ComponentData dependencyData = parameterRegistrationInfo.ComponentDataCollection.FirstOrDefault(p => p.ConcreteType != null && (p.ConcreteType == parameterType || parameterType.IsAssignableFrom(p.ConcreteType)));

						if (dependencyData == null) { continue; }

						// Adiciona uma instancia do tipo encontrado na lista de parâmetros do construtor.
						parameters.Add(CreateInstance(interfaceType, dependencyData, registrationInfo));
					}
				}

				// Verifica se a quantidade de parâmetros esta de acordo com a quantidade de objetos disponíveis.
				if (parameters.Count == parameterInfoCollection.Length) {

					object instance = CreateInstance(registrationInfo, concreteType, parameters.ToArray());

					// Caso algum interceptor tenha sido especificado, utiliza um proxy.
					return (registrationInfo.InterceptorCollection.Any() == false) ? instance : DynamicProxy.NewInstance(instance, interceptors.OfType<IInterceptor>().ToArray());
				}
			}

			throw new InvalidOperationException("Could not find any constructor matching the specified parameters.");
		}

		private static object CreateInstance(IRegistrationInfo registrationInfo, Type concreteType, params object[] args) {

			object returnInstance = Activator.CreateInstance(concreteType, args);

			return returnInstance;
		}
	}
}
