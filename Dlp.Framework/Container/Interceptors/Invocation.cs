using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Framework.Container.Interceptors {

    internal sealed class Invocation : IInvocation {

        public Invocation() { }

        private int _interceptorIndex;

        internal IEnumerable<IInterceptor> Interceptors { get; set; }

        /// <summary>
        /// Gets the arguments passed to the invoked method.
        /// </summary>
        public object[] Arguments { get; set; }

        /// <summary>
        /// Gets the method generic arguments.
        /// </summary>
        public Type[] GenericArguments { get; set; }

        /// <summary>
        /// Gets the information of the method being invoked.
        /// </summary>
        public MethodInfo MethodInvocationTarget { get; set; }

        /// <summary>
        /// Gets the type of the class that contains the invoked method.
        /// </summary>
        public Type TargetType { get; set; }

        /// <summary>
        /// Gets the type of the return value.
        /// </summary>
        public Type ReturnType { get; set; }

        /// <summary>
        /// Gets or sets the value returned by the invoked method.
        /// </summary>
        public object ReturnValue { get; set; }

        internal object InvokedInstance { get; set; }

        internal MethodInfo InvokedMethod { get; set; }

        /// <summary>
        /// Calls the next interceptor. If no more interceptors are available, call the invoked method.
        /// </summary>
        public void Proceed() {

            if (this._interceptorIndex < this.Interceptors.Count()) {

                IInterceptor interceptor = this.Interceptors.ElementAt(this._interceptorIndex);

                this._interceptorIndex++;

                interceptor.Intercept(this);
            }
            else {
                this.ReturnValue = this.MethodInvocation(this.InvokedMethod, this.InvokedInstance, this.Arguments);
            }
        }

        private object MethodInvocation(MethodInfo method, object currentObject, object[] arguments) {

            return method.Invoke(currentObject, arguments);
        }
    }
}
