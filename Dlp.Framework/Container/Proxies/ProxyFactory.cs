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
        private static Object lockObj = new Object();

        private Hashtable typeMap = Hashtable.Synchronized(new Hashtable());
        private static readonly Hashtable opCodeTypeMapper = new Hashtable();

        private const string PROXY_SUFFIX = "Proxy";
        private const string ASSEMBLY_NAME = "ProxyAssembly";
        private const string MODULE_NAME = "ProxyModule";
        private const string HANDLER_NAME = "handler";

        // Initialize the value type mapper.  This is needed for methods with intrinsic 
        // return types, used in the Emit process.
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
            Type type = (Type)typeMap[typeName];

            // check to see if the type was in the cache. If the type was not cached, then create a new instance of the dynamic type and add it to the cache.
            if (type == null) {

                List<Type> interfacesToImplement = new List<Type>();

                // Verifica se foram especificadas interfaces adicionais.
                if (aditionalInterfaces != null && aditionalInterfaces.Any() == true) { interfacesToImplement.AddRange(aditionalInterfaces); }

                if (objType.IsInterface == true) {

                    // gets _all_ parent interfaces!
                    Type[] baseInterfaces = objType.GetInterfaces();

                    if (baseInterfaces != null) {

                        // iterate through the parent interfaces and add them to the list of interfaces to implement.
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

                typeMap.Add(typeName, type);
            }

            // return a new instance of the type.
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

                // create a new assembly for this proxy, one that isn't presisted on the file system
                AssemblyBuilder assemblyBuilder = domain.DefineDynamicAssembly(
                    assemblyName, AssemblyBuilderAccess.Run);

                // create a new module for this proxy
                ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(MODULE_NAME);

                // Set the class to be public and sealed
                TypeAttributes typeAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;

                // Gather up the proxy information and create a new type builder.  One that
                // inherits from Object and implements the interface passed in
                TypeBuilder typeBuilder = moduleBuilder.DefineType(dynamicTypeName, typeAttributes, objType, interfaces);

                // Define a member variable to hold the delegate
                FieldBuilder handlerField = typeBuilder.DefineField(HANDLER_NAME, handlerType, FieldAttributes.Private);

                // build a constructor that takes the delegate object as the only argument
                //ConstructorInfo defaultObjConstructor = objType.GetConstructor( new Type[0] );
                ConstructorInfo superConstructor = objType.GetConstructor(new Type[0]);
                ConstructorBuilder delegateConstructor = typeBuilder.DefineConstructor(
                    MethodAttributes.Public, CallingConventions.Standard, new Type[] { handlerType });

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

                // for every method that the interfaces define, build a corresponding 
                // method in the dynamic type that calls the handlers invoke method.  
                foreach (Type interfaceType in interfaces) {
                    GenerateMethod(interfaceType, handlerField, typeBuilder);
                }

                retVal = typeBuilder.CreateType();
            }

            return retVal;
        }

        private void GenerateMethod(Type interfaceType, FieldBuilder handlerField, TypeBuilder typeBuilder) {

            MetaDataFactory.Add(interfaceType);
			MethodInfo[] interfaceMethods = interfaceType.GetMethods().OrderBy(p => p.Name).ToArray();

            if (interfaceMethods != null) {

                for (int i = 0; i < interfaceMethods.Length; i++) {

                    MethodInfo methodInfo = interfaceMethods[i];

                    // Get the method parameters since we need to create an array of parameter types                         
                    ParameterInfo[] methodParams = methodInfo.GetParameters();
                    int numOfParams = methodParams.Length;
                    Type[] methodParameters = new Type[numOfParams];

                    // convert the ParameterInfo objects into Type
                    for (int j = 0; j < numOfParams; j++) {
                        methodParameters[j] = methodParams[j].ParameterType;
                    }

                    Type[] genericArguments = methodInfo.GetGenericArguments();

                    MethodBuilder methodBuilder = null;

                    if (genericArguments.Length == 0) {

                        // create a new builder for the method in the interface
                        methodBuilder = typeBuilder.DefineMethod(
                            methodInfo.Name,
                            MethodAttributes.Public | MethodAttributes.Virtual,
                            CallingConventions.Standard,
                            methodInfo.ReturnType, methodParameters);
                    }
                    else {

                        methodBuilder = typeBuilder.DefineMethod(
                            methodInfo.Name,
                            MethodAttributes.Public | MethodAttributes.Virtual,
                            CallingConventions.Standard);

                        methodBuilder.SetReturnType(methodInfo.ReturnType);

                        Dictionary<int, string> typeParamNames = new Dictionary<int, string>();
                        Dictionary<int, Type> parameters = new Dictionary<int, Type>();

                        int currentIndex = 0;

                        foreach (ParameterInfo parameterInfo in methodParams) {

                            if (parameterInfo.ParameterType.FullName == null) {
                                typeParamNames.Add(currentIndex, parameterInfo.ParameterType.Name);
                            }
                            else {
                                parameters.Add(currentIndex, parameterInfo.ParameterType);
                            }

                            currentIndex++;
                        }

                        if (typeParamNames.Count > 0) {

                            GenericTypeParameterBuilder[] typeParameters = methodBuilder.DefineGenericParameters(typeParamNames.Values.ToArray());

                            for (int j = 0; j < typeParameters.Length; j++) {
                                int parameterIndex = typeParamNames.ElementAt(j).Key;
                                parameters.Add(parameterIndex, typeParameters[j]);
                            }

                            methodBuilder.SetReturnType(methodInfo.ReturnType);
                        }
                        else {
                            GenericTypeParameterBuilder[] returnParameter = methodBuilder.DefineGenericParameters(methodInfo.ReturnType.Name);
                            methodBuilder.SetReturnType(returnParameter[0]);
                        }

                        IEnumerable<Type> orderedParameters = parameters.OrderBy(p => p.Key).Select(p => p.Value);

                        methodBuilder.SetParameters(orderedParameters.ToArray());
                    }

                    #region( "Handler Method IL Code" )

                    bool hasReturnValue = (methodInfo.ReturnType.Equals(typeof(void)) == false);

                    ILGenerator methodIL = methodBuilder.GetILGenerator();

                    // 20060830 NJ-Always declare object array variable for parameters
                    methodIL.DeclareLocal(typeof(System.Object[]));

                    // Emit a declaration of a local variable if there is a return type defined
                    if (hasReturnValue == true) {

                        methodIL.DeclareLocal(methodInfo.ReturnType);

                        if (methodInfo.ReturnType.IsValueType && (methodInfo.ReturnType.IsPrimitive == false)) {
                            methodIL.DeclareLocal(methodInfo.ReturnType);
                        }
                    }

                    // declare a label for invoking the handler
                    Label handlerLabel = methodIL.DefineLabel();

                    // declare a lable for returning from the mething
                    Label returnLabel = methodIL.DefineLabel();

                    // load "this"
                    methodIL.Emit(OpCodes.Ldarg_0);

                    // load the handler instance variable
                    methodIL.Emit(OpCodes.Ldfld, handlerField);

                    // jump to the handlerLabel if the handler instance variable is not null
                    methodIL.Emit(OpCodes.Brtrue_S, handlerLabel);

                    // the handler is null, so return null if the return type of the method is not void, otherwise return nothing
                    if (hasReturnValue == true) {

                        if (methodInfo.ReturnType.IsValueType && methodInfo.ReturnType.IsPrimitive == false && methodInfo.ReturnType.IsEnum == false) {
                            //methodIL.Emit(OpCodes.Ldloc_1);
                            methodIL.Emit(OpCodes.Ldloc_0);
                        }
                        else {
                            // load null onto the stack
                            methodIL.Emit(OpCodes.Ldnull);
                        }

                        // store the null return value
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

                    // the handler is not null, so continue with execution
                    methodIL.MarkLabel(handlerLabel);

                    // load "this"
                    methodIL.Emit(OpCodes.Ldarg_0);

                    // load the handler
                    methodIL.Emit(OpCodes.Ldfld, handlerField);

                    // load "this" since its needed for the call to invoke
                    methodIL.Emit(OpCodes.Ldarg_0);

                    // load the name of the interface, used to get the MethodInfo object from MetaDataFactory
                    methodIL.Emit(OpCodes.Ldstr, interfaceType.FullName);

                    // load the index, used to get the MethodInfo object from MetaDataFactory 
                    methodIL.Emit(OpCodes.Ldc_I4, i);

                    // invoke GetMethod in MetaDataFactory
                    methodIL.Emit(OpCodes.Call, typeof(MetaDataFactory).GetMethod("GetMethod", new Type[] { typeof(string), typeof(int) }));

                    // load the number of parameters onto the stack
                    methodIL.Emit(OpCodes.Ldc_I4, numOfParams);

                    // create a new array, using the size that was just pushed on the stack
                    methodIL.Emit(OpCodes.Newarr, typeof(System.Object));

                    // if we have any parameters, then iterate through and set the values of each element to the corresponding arguments
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

                    // call the Invoke method
                    methodIL.Emit(OpCodes.Callvirt, typeof(IDynamicProxy).GetMethod("Invoke"));

                    if (hasReturnValue == true) {

                        // if the return type if a value type, then unbox the return value so that we don't get junk.
                        if (methodInfo.ReturnType.IsValueType) {

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

                        // store the result
                        //methodIL.Emit(OpCodes.Stloc_0);
                        methodIL.Emit(OpCodes.Stloc_1);

                        // jump to the return statement
                        methodIL.Emit(OpCodes.Br_S, returnLabel);

                        // mark the return statement
                        methodIL.MarkLabel(returnLabel);

                        // load the value stored before we return.  This will either be null (if the handler was null) or the return value from Invoke
                        //methodIL.Emit(OpCodes.Ldloc_0);
                        methodIL.Emit(OpCodes.Ldloc_1);
                    }
                    else {
                        // pop the return value that Invoke returned from the stack since
                        // the method's return type is void. 
                        methodIL.Emit(OpCodes.Pop);

                        //mark the return statement
                        methodIL.MarkLabel(returnLabel);
                    }

                    // Return
                    methodIL.Emit(OpCodes.Ret);
                    #endregion

                }
            }

            // Iterate through the parent interfaces and recursively call this method
            foreach (Type parentType in interfaceType.GetInterfaces()) { GenerateMethod(parentType, handlerField, typeBuilder); }
        }
    }
}
