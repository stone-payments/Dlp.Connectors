using System;
using System.Reflection;

namespace Dlp.Framework.Container.Proxies {

    public interface IDynamicProxy {

		///<summary>
		/// Method that gets called from within the proxy instance. 
		///</summary>
		///<param name="proxy">Instance of proxy.</param>
		///<param name="method">Method instance.</param>
		///<param name="parameters">Parameters for the method.</param>
		///<param name="genericArguments">List of the generic types of the called method.</param>
        object Invoke(object proxy, MethodInfo methodInfo, object[] parameters, Type[] genericArguments);
    }
}
