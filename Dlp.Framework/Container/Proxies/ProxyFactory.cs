using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dlp.Framework.Container.Proxies {

	internal class ProxyFactory {

		private static ProxyFactory instance;
		private static object lockObj = new object();

		private Hashtable typeMap = Hashtable.Synchronized(new Hashtable());
		private static readonly Hashtable opCodeTypeMapper = new Hashtable();

		private const string PROXY_SUFFIX = "Proxy";
		private const string ASSEMBLY_NAME = "ProxyAssembly";
		private const string MODULE_NAME = "ProxyModule";
		private const string HANDLER_NAME = "handler";

		/// <summary>
		/// Initialize the value type mapper. This is needed for methods with intrinsic return types, used in the Emit process.
		/// </summary>
		static ProxyFactory() {
			opCodeTypeMapper.Add(typeof(System.Boolean), OpCodes.Ldind_I1);
			opCodeTypeMapper.Add(typeof(System.Char), OpCodes.Ldind_I1);
			opCodeTypeMapper.Add(typeof(System.Byte), OpCodes.Ldind_I1);
			opCodeTypeMapper.Add(typeof(System.Int16), OpCodes.Ldind_I2);
			opCodeTypeMapper.Add(typeof(System.Int32), OpCodes.Ldind_I4);
			opCodeTypeMapper.Add(typeof(System.Int64), OpCodes.Ldind_I8);
			opCodeTypeMapper.Add(typeof(System.Double), OpCodes.Ldind_R8);
			opCodeTypeMapper.Add(typeof(System.Single), OpCodes.Ldind_R4);
			opCodeTypeMapper.Add(typeof(System.UInt16), OpCodes.Ldind_U2);
			opCodeTypeMapper.Add(typeof(System.UInt32), OpCodes.Ldind_U4);
		}

		private ProxyFactory() { }

		/// <summary>
		/// Creates a new instance of the ProxyFactory.
		/// </summary>
		/// <returns>Returns a ProxyFactory instance.</returns>
		public static ProxyFactory GetInstance() {

			if (instance == null) { CreateInstance(); }

			return instance;
		}

		private static void CreateInstance() {

			lock (lockObj) {

				if (instance == null) { instance = new ProxyFactory(); }
			}
		}

		//public object Create(IDynamicProxy handler, Type objType, bool isObjInterface) {
		public object Create(IDynamicProxy handler, Type objType, Type[] aditionalInterfaces = null) {

			string typeName = objType.FullName + PROXY_SUFFIX;
			Type type = typeMap[typeName] as Type;

			// Verifica se o tipo já existe no cache de tipos criados dinamicamente. Caso não exista, cria a nova instancia e adiciona ao cache.
			if (type == null) {

				List<Type> interfacesToImplement = new List<Type>();

				// Verifica se foram especificadas interfaces adicionais.
				if (aditionalInterfaces != null && aditionalInterfaces.Any() == true) { interfacesToImplement.AddRange(aditionalInterfaces); }

				if (objType.IsInterface == true) {

					// Pega todas as interfaces que são herdadas.
					Type[] baseInterfaces = objType.GetInterfaces();

					if (baseInterfaces != null) {

						// Adiciona cada interface herdada na lista de interfaces a serem implementadas.
						for (int i = 0; i < baseInterfaces.Length; i++) {
							interfacesToImplement.Add(baseInterfaces[i]);
						}
					}

					interfacesToImplement.Add(objType);
				}
				else {
					interfacesToImplement.AddRange(objType.GetInterfaces());
				}

				// Verifica se foram encontradas interfaces a serem implementadas.
				if (interfacesToImplement == null || interfacesToImplement.Any() == false) {

					throw new ArgumentException(objType.FullName + " has no interfaces to implement", "objType");
				}

				type = CreateType(handler, interfacesToImplement.ToArray(), typeName);

				Type existingType = typeMap[typeName] as Type;
				if (existingType == null) { typeMap.Add(typeName, type); }
			}

			// Retorna uma nova instancia do tipo.
			return Activator.CreateInstance(type, new object[] { handler });
		}

		private Type CreateType(IDynamicProxy handler, Type[] interfaces, string dynamicTypeName) {

			Type retVal = null;

			if (handler != null && interfaces != null) {

				Type objType = typeof(System.Object);
				Type handlerType = typeof(IDynamicProxy);

				AppDomain domain = Thread.GetDomain();
				AssemblyName assemblyName = new AssemblyName();
				assemblyName.Name = ASSEMBLY_NAME;
				assemblyName.Version = new Version(1, 0, 0, 0);

				// Cria um novo assembly para o proxy, que será executado apenas em memória.
				AssemblyBuilder assemblyBuilder = domain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

				// Cria um módulo para conter o proxy.
				ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(MODULE_NAME);

				// Define os atributos do proxy como public e sealed.
				TypeAttributes typeAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;

				// Define as informações do proxy para instanciar um TypeBuilder. O proxy deve herdar de object e das interfaces a serem implementadas.
				TypeBuilder typeBuilder = moduleBuilder.DefineType(dynamicTypeName, typeAttributes, objType, interfaces);

				// Criamos um campo que será utilizado como delegate para executar os métodos originais após a execução do proxy.
				FieldBuilder handlerField = typeBuilder.DefineField(HANDLER_NAME, handlerType, FieldAttributes.Private);

				// Constrói o construtor que recebe o delegate como argumento.
				ConstructorInfo superConstructor = objType.GetConstructor(new Type[0]);
				ConstructorBuilder delegateConstructor = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { handlerType });

				#region( "Constructor IL Code" )
				ILGenerator constructorIL = delegateConstructor.GetILGenerator();

				// Load "this"
				constructorIL.Emit(OpCodes.Ldarg_0);
				// Load first constructor parameter
				constructorIL.Emit(OpCodes.Ldarg_1);
				// Set the first parameter into the handler field
				constructorIL.Emit(OpCodes.Stfld, handlerField);
				// Load "this"
				constructorIL.Emit(OpCodes.Ldarg_0);
				// Call the super constructor
				constructorIL.Emit(OpCodes.Call, superConstructor);
				// Constructor return
				constructorIL.Emit(OpCodes.Ret);
				#endregion

				// Para cada método definido nas interfaces, cria a implementação concreta no tipo dinâmico.
				foreach (Type interfaceType in interfaces) {
					GenerateMethod(interfaceType, handlerField, typeBuilder);
				}

				retVal = typeBuilder.CreateType();
			}

			return retVal;
		}

		private void GenerateMethod(Type interfaceType, FieldBuilder handlerField, TypeBuilder typeBuilder) {

			MetaDataFactory.Add(interfaceType);

			// Obtém a lista de todos os métodos a serem criados.
			MethodInfo[] interfaceMethods = interfaceType.GetMethods().OrderBy(p => p.Name).ToArray();

			// Verifica se existe algum método a ser implementado na interface.
			if (interfaceMethods != null) {

				// Cria cada um dos métodos definidos na interface.
				for (int i = 0; i < interfaceMethods.Length; i++) {

					MethodInfo methodInfo = interfaceMethods[i];

					// Obtém os parâmetros do método que está sendo criado.
					ParameterInfo[] methodParams = methodInfo.GetParameters();
					int numOfParams = methodParams.Length;
					Type[] methodParameters = new Type[numOfParams];

					// Armazena o tipo de cada parâmetro do método.
					for (int j = 0; j < numOfParams; j++) {
						methodParameters[j] = methodParams[j].ParameterType;
					}

					// Armazena todos os tipos genéricos do método.
					Type[] genericArguments = methodInfo.GetGenericArguments();

					MethodBuilder methodBuilder = null;

					// Verifica se o método não possui tipos genéricos.
					if (genericArguments.Length == 0) {

						// Cria um MethodBuilder para o método da interface que esta sendo criado. Como não há tipos genéricos, a definição é feita em apenas uma etapa.
						methodBuilder = typeBuilder.DefineMethod(
							methodInfo.Name,
							MethodAttributes.Public | MethodAttributes.Virtual,
							CallingConventions.Standard,
							methodInfo.ReturnType, methodParameters);
					}
					else {

						// Cria um MethodBuilder que deverá ser preenchido em etapas, pois os tipos genéricos do método precisam ser identificados.
						methodBuilder = typeBuilder.DefineMethod(
							methodInfo.Name,
							MethodAttributes.Public | MethodAttributes.Virtual,
							CallingConventions.Standard);

						// Define o tipo de retorno do método.
						methodBuilder.SetReturnType(methodInfo.ReturnType);

						Dictionary<int, string> typeParamNames = new Dictionary<int, string>();
						Dictionary<int, Type> parameters = new Dictionary<int, Type>();

						int currentIndex = 0;

						// Analisa todos os parâmetros do método.
						foreach (ParameterInfo parameterInfo in methodParams) {

							// Caso o parâmetro não possua a propriedade FullName preenchida, indica que é um tipo genérico. Caso contrário, é um tipo forte.
							if (parameterInfo.ParameterType.FullName == null) {
								typeParamNames.Add(currentIndex, parameterInfo.ParameterType.Name);
							}
							else {
								parameters.Add(currentIndex, parameterInfo.ParameterType);
							}

							currentIndex++;
						}

						// Verifica se existe algum tipo genérico.
						if (typeParamNames.Count > 0) {

							// Informa ao MethodBuilder os nomes dos tipos genéricos.
							GenericTypeParameterBuilder[] typeParameters = methodBuilder.DefineGenericParameters(typeParamNames.Values.ToArray());

							// Adiciona os tipos genéricos na lista de tipos do método.
							for (int j = 0; j < typeParameters.Length; j++) {
								int parameterIndex = typeParamNames.ElementAt(j).Key;
								parameters.Add(parameterIndex, typeParameters[j]);
							}

							// Define o tipo de retorno do método.
							methodBuilder.SetReturnType(methodInfo.ReturnType);
						}
						// Verifica se o tipo de retorno é um tipo genérico.
						else if (methodInfo.ReturnType.IsGenericParameter == true) {

							// Informa ao MethodBuilder o nome do tipo genérico.
							GenericTypeParameterBuilder[] returnParameter = methodBuilder.DefineGenericParameters(methodInfo.ReturnType.Name);

							// Define o tipo de retorno do método.
							methodBuilder.SetReturnType(returnParameter[0]);
						}
						else {
							// Informa ao MethodBuilder os nomes dos tipos genéricos.
							methodBuilder.DefineGenericParameters(genericArguments.Select(p => p.Name).ToArray());
						}

						IEnumerable<Type> orderedParameters = parameters.OrderBy(p => p.Key).Select(p => p.Value);

						// Informa ao MethodBuilder os nomes de todos os parâmetros do método.
						methodBuilder.SetParameters(orderedParameters.ToArray());
					}

					#region( "Handler Method IL Code" )

					bool hasReturnValue = (methodInfo.ReturnType.Equals(typeof(void)) == false);

					ILGenerator methodIL = methodBuilder.GetILGenerator();

					// Sempre declare um array para conter os parâmetros para o método a ser chamado pelo proxy.
					methodIL.DeclareLocal(typeof(System.Object[]));

					// Emite a declaração de uma variável local, caso exista um tipo de retorno para o método.
					if (hasReturnValue == true) {

						methodIL.DeclareLocal(methodInfo.ReturnType);

						//if (methodInfo.ReturnType.IsValueType && (methodInfo.ReturnType.IsPrimitive == false)) {
						//	methodIL.DeclareLocal(methodInfo.ReturnType);
						//}
					}

					// Cria um label para indicar onde o uso do proxy é iniciado.
					Label handlerLabel = methodIL.DefineLabel();

					// Cria um label para indicar onde está a saída do método (return).
					Label returnLabel = methodIL.DefineLabel();

					// Carrega "this" no stack.
					methodIL.Emit(OpCodes.Ldarg_0);

					// Carrega o handler para o proxy.
					methodIL.Emit(OpCodes.Ldfld, handlerField);

					// Caso exista um proxy, pula para o label que será utilizado para executá-lo.
					methodIL.Emit(OpCodes.Brtrue_S, handlerLabel);

					// Verifica se o método possui retorno.
					if (hasReturnValue == true) {

						// Caso seja um tipo primitivo apenas cria a variável de retorno, com o valor default do tipo.
						if (methodInfo.ReturnType.IsValueType && methodInfo.ReturnType.IsPrimitive == false && methodInfo.ReturnType.IsEnum == false
							&& (methodInfo.ReturnType.IsGenericType && methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Nullable<>)) == false) {
							//methodIL.Emit(OpCodes.Ldloc_1);
							methodIL.Emit(OpCodes.Ldloc_0);
						}
						// Caso o retorno seja uma classe, carrega nulo na variável de retorno.
						else {
							methodIL.Emit(OpCodes.Ldnull);
						}

						// Armazena o valor de retorno.
						methodIL.Emit(OpCodes.Stloc_0);

						// Passa para o fim do método.
						//methodIL.Emit(OpCodes.Br_S, returnLabel);
						methodIL.Emit(OpCodes.Br, returnLabel);
					}
					else {
						// Passa para o fim do método.
						//methodIL.Emit(OpCodes.Br_S, returnLabel);
						methodIL.Emit(OpCodes.Br, returnLabel);
					}

					// Define o label que indica o início do trecho de execução do proxy.
					methodIL.MarkLabel(handlerLabel);

					// Carrega "this" no stack.
					methodIL.Emit(OpCodes.Ldarg_0);

					// Carrega o handler para o IDinamicProxy.
					methodIL.Emit(OpCodes.Ldfld, handlerField);

					// Carrega "this" no stack. Será utilizado para chamar o método GetMethod mais abaixo.
					methodIL.Emit(OpCodes.Ldarg_0);

					// Carrega o nome da interface no stack.
					methodIL.Emit(OpCodes.Ldstr, interfaceType.FullName);

					// Carrega o índice do método no stack.
					methodIL.Emit(OpCodes.Ldc_I4, i);

					// Executa o método GetMethod da classe MetaDataFactory, passando como parâmetros o nome da interface e o índice do método, carregados no stack.
					methodIL.Emit(OpCodes.Call, typeof(MetaDataFactory).GetMethod("GetMethod", new Type[] { typeof(string), typeof(int) }));

					// Carrega a quantidade de parâmetros do método no topo do stack.
					methodIL.Emit(OpCodes.Ldc_I4, numOfParams);

					// Cria um array de objetos. A quantidade de parâmetros carregados no stack é utilizado para definir o tamanho do array.
					methodIL.Emit(OpCodes.Newarr, typeof(System.Object));

					// Adiciona cada parâmetro existente na lista de parâmetros que serão utilizados ao invocar o método concreto.
					if (numOfParams > 0) {

						//methodIL.Emit(OpCodes.Stloc_1);
						methodIL.Emit(OpCodes.Stloc_0);

						for (int j = 0; j < numOfParams; j++) {

							//methodIL.Emit(OpCodes.Ldloc_1);
							methodIL.Emit(OpCodes.Ldloc_0);
							methodIL.Emit(OpCodes.Ldc_I4, j);
							methodIL.Emit(OpCodes.Ldarg, j + 1);

							if (methodParameters[j].IsValueType) {
								methodIL.Emit(OpCodes.Box, methodParameters[j]);
							}

							methodIL.Emit(OpCodes.Stelem_Ref);
						}

						//methodIL.Emit(OpCodes.Ldloc_1);
						methodIL.Emit(OpCodes.Ldloc_0);
					}

					// Carrega a quantidade de argumentos genéricos para criar o próximo array.
					methodIL.Emit(OpCodes.Ldc_I4, genericArguments.Length);

					// Cria um array que conterá todos os tipos genéricos do método.
					methodIL.Emit(OpCodes.Newarr, typeof(System.Type));

					// Adicionamos o tipo de cada parâmetro genérico no array.
					if (genericArguments.Length > 0) {

						// Salva o array.
						methodIL.Emit(OpCodes.Stloc_0);

						// Obtém cada parâmetro genérico a ser adicionado.
						for (int j = 0; j < genericArguments.Length; j++) {

							// Carrega o array.
							methodIL.Emit(OpCodes.Ldloc_0);

							// Seleciona a posição j do array.
							methodIL.Emit(OpCodes.Ldc_I4, j);

							// Carrega o item a ser adicionado no array.
							methodIL.Emit(OpCodes.Ldtoken, genericArguments[j]);

							// Caso o parâmetro seja value type, é preciso fazer box.
							if (genericArguments[j].IsValueType) {
								methodIL.Emit(OpCodes.Box, genericArguments[j]);
							}

							// Insere o item no array.
							methodIL.Emit(OpCodes.Stelem_Ref);
						}

						// Carrega o array preenchido, para que seja usado no método a seguir.
						methodIL.Emit(OpCodes.Ldloc_0);
					}

					// Chama o método concreto através do Invoke.
					methodIL.Emit(OpCodes.Callvirt, typeof(IDynamicProxy).GetMethod("Invoke"));

					// Verifica se o método possui returno.
					if (hasReturnValue == true) {

						// Faz o unbox do valor de retorno, caso seja um valueType ou generic.
						if (methodInfo.ReturnType.IsValueType == true || methodInfo.ReturnType.IsGenericParameter == true) {

							methodIL.Emit(OpCodes.Unbox, methodInfo.ReturnType);
							if (methodInfo.ReturnType.IsEnum) {
								methodIL.Emit(OpCodes.Ldind_I4);
							}
							else if (methodInfo.ReturnType.IsPrimitive == false) {
								methodIL.Emit(OpCodes.Ldobj, methodInfo.ReturnType);
							}
							else {
								methodIL.Emit((OpCode)opCodeTypeMapper[methodInfo.ReturnType]);
							}
						}
						else {

							methodIL.Emit(OpCodes.Castclass, methodInfo.ReturnType);
						}

						// Armazena o valor retornado pelo método Invoke.
						//methodIL.Emit(OpCodes.Stloc_0);
						methodIL.Emit(OpCodes.Stloc_1);

						// Pula para o label de saída do método.
						//methodIL.Emit(OpCodes.Br_S, returnLabel);
						methodIL.Emit(OpCodes.Br, returnLabel);

						// Define o label para o ponto de saída do método;
						methodIL.MarkLabel(returnLabel);

						// Carrega o valor armazenado antes de retornar. Será nulo, caso não exista retorno, ou será o valor retornado pelo método Invoke.
						//methodIL.Emit(OpCodes.Ldloc_0);
						methodIL.Emit(OpCodes.Ldloc_1);
					}
					else {
						// Como o método não possui retorno, remove qualquer valor armazenado antes de retornar. 
						methodIL.Emit(OpCodes.Pop);

						// Define o label para o ponto de saída do método;
						methodIL.MarkLabel(returnLabel);
					}

					// Define o ponto de saída do método (return);
					methodIL.Emit(OpCodes.Ret);
					#endregion

				}
			}

			// Chama todas as demais interfaces herdadas pela interface atual, recursivamente.
			foreach (Type parentType in interfaceType.GetInterfaces()) { GenerateMethod(parentType, handlerField, typeBuilder); }
		}
	}
}
