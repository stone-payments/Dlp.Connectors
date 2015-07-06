using System;
using System.Reflection;

namespace Dlp.Framework.Container.Interceptors {

    public interface IInvocation {

        /// <summary>
        /// Gets the arguments passed to the invoked method.
        /// </summary>
        object[] Arguments { get; }

        /// <summary>
        /// Gets the method generic arguments.
        /// </summary>
        Type[] GenericArguments { get; }

        /// <summary>
        /// Gets the information of the method being invoked.
        /// </summary>
        MethodInfo MethodInvocationTarget { get; }

        /// <summary>
        /// Gets the type of the class that contains the invoked method.
        /// </summary>
        Type TargetType { get; }

        /// <summary>
        /// Gets the type of the return value.
        /// </summary>
        Type ReturnType { get; }

        /// <summary>
        /// Gets or sets the returned value from the invoked method.
        /// </summary>
        object ReturnValue { get; set; }

        /// <summary>
        /// Calls the next interceptor. If no more interceptors are available, call the invoked method.
        /// </summary>
        void Proceed();
    }
}
