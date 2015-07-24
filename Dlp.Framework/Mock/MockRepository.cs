using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Framework.Mock {

    internal static class MockRepository {

		private static Dictionary<string, List<IMethodOptions>> methodOptionsDictionary;
		private static Dictionary<string, List<IMethodOptions>> MethodOptionsDictionary {
			get {
				if (methodOptionsDictionary == null) { methodOptionsDictionary = new Dictionary<string, List<IMethodOptions>>(); }
				return methodOptionsDictionary;
			}
		}

        internal static void Add(string mock, IMethodOptions methodOptions) {

            List<IMethodOptions> methodOptionsList = null;

            // Cria o registro para o mock especificado.
            if (MethodOptionsDictionary.ContainsKey(mock) == false) {

                methodOptionsList = new List<IMethodOptions>();
                MethodOptionsDictionary.Add(mock, methodOptionsList);
            }
            else{
                methodOptionsList = MethodOptionsDictionary[mock];
            }

            // Tenta localizar as opções para o método especificado.
            //IMethodOptions actualMethodOptions = methodOptionsList.FirstOrDefault(p => p.MethodName == methodOptions.MethodName);
			IMethodOptions actualMethodOptions = methodOptionsList.FirstOrDefault(p => p.MemberFullName == methodOptions.MemberFullName);

            // Caso as opções sejam encontradas, atualiza o método. Caso contrário, insere as novas opções.
            if (actualMethodOptions != null) { actualMethodOptions = methodOptions; }
            else { methodOptionsList.Add(methodOptions); }
        }

		internal static IMethodOptions Load(string mock, string methodName, object[] arguments = null) {

			if (MethodOptionsDictionary.ContainsKey(mock) == false) {
				return null;
			}

			string parsedArguments = string.Empty;

			if (arguments != null) {

				for (int i = 0; i < arguments.Length; i++) {

					if (arguments[i] == null) {
						parsedArguments += "null";
					}
					else if (arguments[i].GetType().FullName.StartsWith("System.") == false) {
						parsedArguments += Serializer.JsonSerialize(arguments[i]);
					}
					else {
						parsedArguments += arguments[i].ToString();
					}

					// Caso existam mais parâmetros, adiciona um separador para o próximo parâmetro.
					if (i < arguments.Length - 1) {
						parsedArguments += ", ";
					}
				}
			}

			string md5Arguments = parsedArguments.CalculateMd5(mock);

			//string memberFullName = (arguments != null) ? string.Format("{0}({1})", methodName, string.Join(", ", arguments)) : methodName;
			string memberFullName = (arguments != null) ? string.Format("{0}({1})", methodName, md5Arguments) : methodName;

			IMethodOptions methodOptions = MethodOptionsDictionary[mock].FirstOrDefault(p => p.MemberFullName == memberFullName);

			return methodOptions;
		}
    }
}
