using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Framework.Mock {

	public delegate R Function<T, R>(T t);

	public static class MockerExtensions {

		//public static IMethodOptions Stub<T>(this T mockObject, Expression<Action<T>> action) where T : class {

		//	throw new NotImplementedException();
		//}

		public static IMethodOptions<R> Stub<T, R>(this T mockObject, Expression<Func<T, R>> action) where T : class {

			if (mockObject == null) {
				throw new ArgumentNullException("mockObject", "You cannot mock a null instance.");
			}

			string mockName = mockObject.GetType().FullName;

			string memberName = null;

			string memberFullName = null;

			string parametersMd5 = string.Empty;

			if (action.Body is MethodCallExpression) {

				MethodCallExpression methodCallExpression = (MethodCallExpression)action.Body;
				memberName = methodCallExpression.Method.Name;

				memberFullName = memberName + "(";

				ReadOnlyCollection<Expression> args = methodCallExpression.Arguments;

				// Processa todos os parâmetros, executando qualquer método que tenha sido especificado e obtendo seu resultado.
				for (int i = 0; i < methodCallExpression.Arguments.Count; i++) {

					Expression argumentExpression = methodCallExpression.Arguments[i];

					// Verifica se o parâmetro é uma chamada a um método. Caso positivo, o método será executado e o resultado será usado como parâmetro.
					if (argumentExpression is MethodCallExpression || argumentExpression is MemberExpression) {

						// Executa o método e obtém o resultado.
						object result = Expression.Lambda(argumentExpression).Compile().DynamicInvoke();

						// Adiciona o resultado no nome completo para identificar o método principal.
						memberFullName += result.ToString().Trim('"');

						parametersMd5 += Serializer.JsonSerialize(result);
					}
					else {
						memberFullName += argumentExpression.ToString().Trim('"');

						parametersMd5 += argumentExpression.ToString().Trim('"');
					}

					// Caso existam mais parâmetros, adiciona um separador para o próximo parâmetro.
					if (i < methodCallExpression.Arguments.Count - 1) {
						
						memberFullName += ", ";

						parametersMd5 += ", ";
					}
				}

				memberFullName += ")";
			}
			else if (action.Body is MemberExpression) {

				MemberExpression memberExpression = (MemberExpression)action.Body;
				memberName = memberExpression.Member.Name;
				memberFullName = memberName;
			}

			string _fullName = memberName;

			if (string.IsNullOrWhiteSpace(parametersMd5) == false) {

				parametersMd5 = parametersMd5.CalculateMd5(mockName);

				_fullName += "(" + parametersMd5 + ")";

				memberFullName = _fullName;
			}

			// Carrega as configurações do método.
			IMethodOptions methodOptions = MockRepository.Load(mockName, memberFullName);

			// Caso ainda não existam opções definidas, cria um novo objeto e adiciona ao repositório.
			if (methodOptions == null) {

				methodOptions = new MethodOptions<R>();
				methodOptions.MemberFullName = memberFullName;
				methodOptions.MethodName = memberName;
				MockRepository.Add(mockName, methodOptions);
			}

			// TODO: Redefinir as opções, caso já exista o IMethodOptions.

			return methodOptions as IMethodOptions<R>;
		}

		private static Expression<Function<T, R>> CreateExpressionFromFunction<T, R>(Function<T, R> action) {

			return x => action(x);
		}
	}
}
