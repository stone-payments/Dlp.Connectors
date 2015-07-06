using Dlp.Framework.Container;
using Dlp.Framework.Container.Interceptors;
using Dlp.Framework.Container.Proxies;
using System;

namespace Dlp.Framework.Mock {

	public static class Mocker {

		public static TInterface CreateMock<TInterface>(bool autoRegisterToContainer = false) where TInterface : class {

			TInterface mock = DynamicProxy.NewInstance<TInterface>(new IInterceptor[] { new MockerInterceptor() }, new Type[] { typeof(IMockObject) }) as TInterface;

			// Verifica se o mock deve ser registrado no container de injeção de dependencia.
			if (autoRegisterToContainer == true) {

				IocFactory.Register(
					Component.For<TInterface>().Instance(mock)
					);
			}

			return mock;
		}
	}
}
