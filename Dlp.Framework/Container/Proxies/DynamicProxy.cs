using Dlp.Framework.Container.Interceptors;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace Dlp.Framework.Container.Proxies {

	internal sealed class DynamicProxy : IDynamicProxy {

		private object _currentObject = null;

		private IInterceptor[] _interceptors = null;

		///<summary>
		/// Initializes a new instance of the interceptor.
		///</summary>
		///<param name="objectInstance">Instance of object to be proxied.</param>
		private DynamicProxy(object objectInstance, IInterceptor[] interceptors = null) {

			this._currentObject = objectInstance;
			this._interceptors = interceptors;
		}

		///<summary>
		/// Factory method to create a new proxy instance.
		///</summary>
		///<param name="objectInstance">Instance of object to be proxied</param>
		internal static object NewInstance(object objectInstance, IInterceptor[] interceptors = null, Type[] aditionalInterfaces = null) {

			return ProxyFactory.GetInstance().Create(new DynamicProxy(objectInstance, interceptors), objectInstance.GetType(), aditionalInterfaces);
		}

		internal static object NewInstance<TInterface>(IInterceptor[] interceptors = null, Type[] aditionalInterfaces = null) {

			return ProxyFactory.GetInstance().Create(new DynamicProxy(null, interceptors), typeof(TInterface), aditionalInterfaces);
		}

		///<summary>
		/// IDynamicProxy method that gets called from within the proxy instance. 
		///</summary>
		///<param name="proxy">Instance of proxy.</param>
		///<param name="method">Method instance.</param>
		///<param name="parameters">Parameters for the method.</param>
		public object Invoke(object proxy, MethodInfo method, object[] parameters, Type[] genericArguments) {

			Invocation invocation = new Invocation();
			invocation.Arguments = parameters;
			invocation.TargetType = (this._currentObject ?? proxy).GetType();
			invocation.GenericArguments = genericArguments;

			// Verifica se o método evocado é genérico.
			if (method.IsGenericMethod == true) { method = method.MakeGenericMethod(genericArguments); }

			invocation.MethodInvocationTarget = method;
			invocation.ReturnType = method.ReturnType;

			// Verifica se foi encontrado algum interceptor registrado para o tipo.
			if (this._interceptors.Any() == true) {

				invocation.InvokedMethod = method;
				invocation.InvokedInstance = this._currentObject;
				invocation.Interceptors = this._interceptors;

				invocation.Proceed();
			}
			else {

				// Chama o método solicitado, passando os parâmetros do InterceptorInput, que podem ter sido modificados.
				invocation.ReturnValue = method.Invoke(this._currentObject ?? proxy, invocation.Arguments);
			}

			return invocation.ReturnValue;
		}
	}
}
