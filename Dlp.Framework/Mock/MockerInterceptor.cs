using Dlp.Framework.Container.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Framework.Mock {

    public sealed class MockerInterceptor : IInterceptor {

        public void Intercept(IInvocation invocation) {

            IMethodOptions methodOptions = null;

            if (typeof(IMockObject).IsAssignableFrom(invocation.TargetType) == true) {

                string mockName = invocation.TargetType.FullName;

				// Verifica se o método chamado é o get_accessor de uma propriedade. Caso positivo, utilizaremos a propriedade, ao invés do método.
				PropertyInfo propertyInfo = invocation.MethodInvocationTarget.DeclaringType.GetProperties().FirstOrDefault(p => p.GetMethod == invocation.MethodInvocationTarget);

				string memberName = (propertyInfo != null) ? propertyInfo.Name : invocation.MethodInvocationTarget.Name;

				methodOptions = MockRepository.Load(mockName, memberName, (propertyInfo != null) ? null : invocation.Arguments);
            }

            // Verifica se existem configurações para o processamento do método. Caso negativo, sai do método.
            if (methodOptions == null) {

                if (invocation.ReturnType != typeof(void)) {

                    if (invocation.ReturnType.IsValueType) {
                        invocation.ReturnValue = Activator.CreateInstance(invocation.ReturnType);
                    }
                    else {
                        invocation.ReturnValue = null;
                    }
                }

                return;
            }

            if (invocation.ReturnType != typeof(void)) {				

                invocation.ReturnValue = methodOptions.ReturnValue;
            }
        }
    }
}
