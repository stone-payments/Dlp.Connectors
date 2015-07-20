using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Framework.Container.Proxies {

	/// <summary>
	/// Factory class used to cache Types instances
	/// </summary>
	public class MetaDataFactory {

		private static Hashtable typeMap = new Hashtable();

		/// <summary>
		/// Class constructor. Private because this is a static class.
		/// </summary>
		private MetaDataFactory() { }

		///<summary>
		/// Method to add a new Type to the cache, using the type's fully qualified name as the key
		///</summary>
		///<param name="interfaceType">Type to cache</param>
		public static void Add(Type interfaceType) {

			// Caso a interface não tenha sido especificada, sai do método.
			if (interfaceType == null) { return; }

			// Bloqueamos a lista de tipos existentes, para garantir que ela não será modificada enquanto estiver sendo lida.
			lock (typeMap.SyncRoot) {

				if (typeMap.ContainsKey(interfaceType.FullName) == false) {
					typeMap.Add(interfaceType.FullName, interfaceType);
				}
			}
		}

		///<summary>
		/// Method to return the method of a given type at a specified index.
		///</summary>
		///<param name="name">Fully qualified name of the method to return</param>
		///<param name="i">Index to use to return MethodInfo</param>
		///<returns>MethodInfo</returns>
		public static MethodInfo GetMethod(string name, int i) {

			Type type = null;

			// Bloqueamos a lista de tipos existentes, para garantir que ela não será modificada enquanto estiver sendo lida.
			lock (typeMap.SyncRoot) {
				type = (Type)typeMap[name];
			}

			MethodInfo[] methods = type.GetMethods().OrderBy(p => p.Name).ToArray();

			if (i < methods.Length) { return methods[i]; }

			return null;
		}
	}
}
