using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Runtime.CompilerServices;
using Dlp.Framework.Container;
using Dlp.Framework.Container.Interceptors;
using System.Reflection;
using Dlp.Sdk.Tests.Framework.Mocks;
using System.Collections.Generic;

namespace Dlp.Sdk.Tests.Framework {

	[TestClass]
	public class IocFactoryInterceptorTest {

		[TestMethod]
		public void Intercept_Test() {

			IocFactory.Register(

				Component.For<IConfigurationUtilityMock>()
				.ImplementedBy<ConfigurationUtilityMock>(),

				Component.For<ILog>()
				.ResolveDependencies()
				.ImplementedBy<FileLog>()
				.ImplementedBy<EventViewerLog>()
				.Interceptor<LogInterceptor>()
			);

			ILog log = IocFactory.Resolve<ILog>();
			log.Save("Test", "Test Message");
		}

		[TestMethod]
		public void IocFactory_Interceptor_Test() {

			IocFactory.Register(
				Component.For<IConfigurationUtilityMock>()
				.ImplementedBy<ConfigurationUtilityMock>()
				.Interceptor<LogInterceptor>()
				);

			IConfigurationUtilityMock mock = IocFactory.Resolve<IConfigurationUtilityMock>();

			Assert.IsNotNull(mock);

			mock.TestValue = "Teste";

			Assert.AreEqual("Intercepted Teste", mock.TestValue);

			string name = mock.GetName("Terra");

			Assert.AreEqual("Intercepted Terra Mock 1", name);

			string testValue = mock.TestValue;

			Assert.AreEqual("Intercepted Teste", testValue);
		}
	}

	public interface ILog {
		
		IConfigurationUtilityMock ConfigurationUtility { get; set; }

		[Intercept]
		void Save(string logCategory, object objectToLog, [CallerMemberName] string methodName = "");
	}

	public abstract class AbstractLog : ILog {

		public IConfigurationUtilityMock ConfigurationUtility { get; set; }

		public void Save(string logCategory, object objectToLog, [CallerMemberName] string methodName = "") {

			try {
				this.SaveImplementation(logCategory, objectToLog, methodName);
			}
			catch (Exception ex) {

			}
		}

		protected abstract void SaveImplementation(string logCategory, object objectToLog, string methodName);
	}

	public class FileLog : AbstractLog {

		public FileLog() { }

		protected override void SaveImplementation(string logCategory, object objectToLog, string methodName) {

			IConfigurationUtilityMock mock = this.ConfigurationUtility;
		}
	}

	public class EventViewerLog : AbstractLog {

		public EventViewerLog() { }

		protected override void SaveImplementation(string logCategory, object objectToLog, string methodName) {

		}
	}

	public sealed class LogInterceptor : IInterceptor {

		public void Intercept(IInvocation invocation) {

			IEnumerable<Attribute> atts = invocation.MethodInvocationTarget.GetCustomAttributes();

			//if (Attribute.IsDefined(invocation.MethodInvocationTarget, typeof(InterceptAttribute)) == false) {

			//	invocation.Proceed();
			//}
			//else {

				MethodInfo methodInfo = invocation.MethodInvocationTarget;
				object[] pars = invocation.Arguments;
				Type type = invocation.TargetType;

				if (invocation.Arguments.Length > 0 && invocation.Arguments[0] is string) {
					invocation.Arguments[0] = "Intercepted " + invocation.Arguments[0];
				}

				invocation.Proceed();
			//}
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public sealed class InterceptAttribute : Attribute {

		public InterceptAttribute() { }
	}
}
