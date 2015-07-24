using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dlp.Framework.Container;
using Dlp.Sdk.Tests.Framework.Mocks;
using System.Reflection;
using Dlp.Framework.Container.Interceptors;
using System.Runtime.CompilerServices;

namespace Dlp.Sdk.Tests.Framework {

	[TestClass]
	public class IocFactoryTest {

		[TestInitialize]
		public void Prepare() {

			IocFactory.Reset();
		}

		[TestMethod]
		public void RegistrationInfo_RegisterInterface_Test() {

			IRegistrationInfo registrationInfo = Component.For<IConfigurationUtilityMock>();

			Assert.IsNotNull(registrationInfo);
			Assert.IsNotNull(registrationInfo.InterfaceType);
			Assert.IsTrue(registrationInfo.ComponentDataCollection.Count() == 0);
		}

		[TestMethod]
		public void RegistrationInfo_RegisterTypeToInterface_Test() {

			string expectedName = typeof(ConfigurationUtilityMock).FullName;

			IRegistrationInfo registrationInfo = Component.For<IConfigurationUtilityMock>()
				.ImplementedBy<ConfigurationUtilityMock>();

			Assert.IsNotNull(registrationInfo);
			Assert.IsNotNull(registrationInfo.InterfaceType);
			Assert.IsTrue(registrationInfo.ComponentDataCollection.Count() == 1);
			Assert.IsNotNull(registrationInfo.ComponentDataCollection.SingleOrDefault(p => expectedName.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase)));
		}

		[TestMethod]
		public void RegistrationInfo_RegisterMultipleTypesToInterface_Test() {

			IRegistrationInfo registrationInfo = Component.For<IConfigurationUtilityMock>()
				.ImplementedBy<ConfigurationUtilityMock>()
				.ImplementedBy<AnotherMock>();

			Assert.IsNotNull(registrationInfo);
			Assert.IsNotNull(registrationInfo.InterfaceType);
			Assert.IsTrue(registrationInfo.ComponentDataCollection.Count() == 2);
		}

		[TestMethod]
		public void RegistrationInfo_RegisterWithCustomName_Test() {

			string expectedName = "Banana";

			IRegistrationInfo registrationInfo = Component.For<IConfigurationUtilityMock>()
				.ImplementedBy<ConfigurationUtilityMock>(expectedName);

			Assert.IsNotNull(registrationInfo);
			Assert.IsNotNull(registrationInfo.InterfaceType);
			Assert.IsTrue(registrationInfo.ComponentDataCollection.Count() == 1);
			Assert.IsNotNull(registrationInfo.ComponentDataCollection.SingleOrDefault(p => expectedName.Equals(p.Name, StringComparison.InvariantCultureIgnoreCase)));
		}

		[TestMethod]
		public void RegistrationInfo_RegisterTypeAsSingleton_Test() {

			IRegistrationInfo registrationInfo = Component.For<IConfigurationUtilityMock>()
				.ImplementedBy<ConfigurationUtilityMock>()
				.IsSingleton();

			Assert.IsNotNull(registrationInfo);
			Assert.IsNotNull(registrationInfo.InterfaceType);
			Assert.IsTrue(registrationInfo.ComponentDataCollection.Count() == 1);
			Assert.IsTrue(registrationInfo.ComponentDataCollection.First().IsSingleton);
		}

		[TestMethod]
		public void RegistrationInfo_RegisterWithInterceptor_Test() {

			IRegistrationInfo registrationInfo = Component
				.For<IConfigurationUtilityMock>()
				.ImplementedBy<ConfigurationUtilityMock>()
				.Interceptor<LogInterceptor>();

			Assert.IsNotNull(registrationInfo);
			Assert.IsNotNull(registrationInfo.InterfaceType);
			Assert.IsTrue(registrationInfo.ComponentDataCollection.Count() == 1);
		}

		[TestMethod]
		public void RegistrationInfo_RegisterNamespace_Test() {

			AssemblyInfo registration = Component
				.FromThisAssembly("Dlp.Sdk.Tests.Framework.Mocks")
				.Interceptor<LogInterceptor>()
				.IsSingleton<AnotherMock>();
		}

		[TestMethod]
		public void IocFactory_Register_Test() {

			IocFactory.Register(
				Component.For<IConfigurationUtilityMock>()
				.ImplementedBy<ConfigurationUtilityMock>()
				.ImplementedBy<AnotherMock>()
				);
		}

		[TestMethod]
		public void IocFactory_Resolve_Test() {

			IocFactory.Register(
					Component.For<IConfigurationUtilityMock>()
					.ImplementedBy<ConfigurationUtilityMock>()
				);

			IConfigurationUtilityMock mock = IocFactory.Resolve<IConfigurationUtilityMock>();

			Assert.IsNotNull(mock);

			mock.TestValue = "Banana";

			IConfigurationUtilityMock mock2 = IocFactory.Resolve<IConfigurationUtilityMock>();

			Assert.IsNotNull(mock2);
			Assert.IsNull(mock2.TestValue);
		}

		[TestMethod]
		public void IocFactory_ResolveSingleton_Test() {

			IocFactory.Register(
					Component.For<IConfigurationUtilityMock>()
					.ImplementedBy<ConfigurationUtilityMock>()
					.IsSingleton()
				);

			IConfigurationUtilityMock mock = IocFactory.Resolve<IConfigurationUtilityMock>();

			Assert.IsNotNull(mock);

			mock.TestValue = "Banana";

			IConfigurationUtilityMock mock2 = IocFactory.Resolve<IConfigurationUtilityMock>();

			Assert.IsNotNull(mock2);
			Assert.AreEqual("Banana", mock2.TestValue);
		}

		[TestMethod]
		public void IocFactory_ResolveContructorParameter_Test() {

			IocFactory.Register(
					Component.For<IConfigurationUtilityMock>()
					.ImplementedBy<ConfigurationUtilityMock>()
				);

			IConfigurationUtilityMock mock = IocFactory.Resolve<IConfigurationUtilityMock>("connectionString");

			Assert.IsNotNull(mock);
		}

		[TestMethod]
		public void IocFactory_ResolveConstructorParameter2_Test() {

			IocFactory.Register(
					Component.For<IConfigurationUtilityMock>()
					.ImplementedBy<ConfigurationUtilityMock>()
				);

			IConfigurationUtilityMock mock = IocFactory.Resolve<IConfigurationUtilityMock>(DateTime.Now);

			Assert.IsNotNull(mock);
		}

		[TestMethod]
		public void IocFactory_ResolveWithParameterRegisteredDependency_Test() {

			IocFactory.Register(
				Component.For<IConfigurationUtilityMock>()
					.ImplementedBy<ConfigurationUtilityMock>()
					.ImplementedBy<AnotherMock>()
				);

			IConfigurationUtilityMock mock = IocFactory.Resolve<IConfigurationUtilityMock>();

			Assert.IsNotNull(mock);
		}

		[TestMethod]
		public void IocFactory_ResolveByName_Test() {

			IocFactory.Register(
				Component.For<IConfigurationUtilityMock>()
				.ImplementedBy<ConfigurationUtilityMock>()
				.ImplementedBy<AnotherMock>()
				);

			IConfigurationUtilityMock mock = IocFactory.ResolveByName<IConfigurationUtilityMock>("Dlp.Sdk.Tests.Framework.Mocks.AnotherMock");

			Assert.IsNotNull(mock);
			Assert.IsInstanceOfType(mock, typeof(AnotherMock));
		}

		[TestMethod]
		public void IocFactory_ResolvedNamed_Test() {

			IocFactory.Register(
				Component.For<IConfigurationUtilityMock>()
				.ImplementedBy<ConfigurationUtilityMock>()
				.ImplementedBy<AnotherMock>("Another")
				);

			IConfigurationUtilityMock mock = IocFactory.ResolveByName<IConfigurationUtilityMock>("Another");

			Assert.IsNotNull(mock);
			Assert.IsInstanceOfType(mock, typeof(AnotherMock));
		}

		//[TestMethod]
		//public void Proxy_Test() {

		//    Assembly assembly = typeof(Dlp.Framework.Container.Proxies.IDynamicProxy).Assembly;

		//    PrivateType privateObject = new PrivateType(assembly.FullName, "Dlp.Framework.Container.Proxies.DynamicProxy");

		//    IConfigurationUtilityMock mock = privateObject.InvokeStatic("NewInstance", new ConfigurationUtilityMock(),
		//        new IInterceptor[] { new LogInterceptor(), new ValidationInterceptor() }) as IConfigurationUtilityMock;

		//    string result = mock.GetName("Banana");

		//    Assert.AreEqual("Fake Cebola Mock 1", result);
		//}

		[TestMethod]
		public void IocFactory_MultipleComponentsForSameInterface_Test() {

			IocFactory.Register(Component.For<IPerson>().ImplementedBy<Pedro>("P"));
			IocFactory.Register(Component.For<IPerson>().ImplementedBy<Odilon>("O"));
			IocFactory.Register(Component.For<IPerson>().ImplementedBy<Guilherme>("G"));

			IPerson person = IocFactory.ResolveByName<IPerson>("O");

			Assert.IsNotNull(person);
			Assert.IsInstanceOfType(person, typeof(Odilon));
		}

		[TestMethod]
		public void IocFactory_MultipleImplementationsForSameInterface() {

			IocFactory.Register(
				Component.For<IPerson>()
					.ImplementedBy<Pedro>("P")
					.ImplementedBy<Odilon>("O")
					.ImplementedBy<Guilherme>("G")
				);

			IPerson person = IocFactory.ResolveByName<IPerson>("O");

			Assert.IsNotNull(person);
			Assert.IsInstanceOfType(person, typeof(Odilon));
		}

		[TestMethod]
		public void IocFactory_RegisterInstance() {

			IPerson pedro = new Pedro();

			pedro.Age = 90;

			IocFactory.Register(
					Component.For<IPerson>()
						.ImplementedBy<Odilon>()
						.Instance(pedro).IsDefault()
						.ImplementedBy<Guilherme>()
				);

			IPerson person = IocFactory.Resolve<IPerson>();

			Assert.IsNotNull(person);
			Assert.IsInstanceOfType(person, typeof(Pedro));
			Assert.AreEqual(90, person.Age);
		}

		[TestMethod]
		public void IocFactory_RegisterMultipleComponents_Test() {

			IocFactory.Register(
					Component.For<IPerson>().ImplementedBy<Pedro>("P"),
					Component.For<IPerson>().ImplementedBy<Odilon>("O"),
					Component.For<IPerson>().ImplementedBy<Guilherme>("G")
				);

			IPerson person = IocFactory.ResolveByName<IPerson>("O");

			Assert.IsNotNull(person);
			Assert.IsInstanceOfType(person, typeof(Odilon));
		}

		[TestMethod]
		public void IocFactory_GetDefaultType_Test() {

			IocFactory.Register(
					Component.For<IPerson>().ImplementedBy<Pedro>("P"),
					Component.For<IPerson>().ImplementedBy<Odilon>("O").IsDefault(),
					Component.For<IPerson>().ImplementedBy<Guilherme>("G")
				);

			IPerson person = IocFactory.Resolve<IPerson>();

			Assert.IsNotNull(person);
			Assert.IsInstanceOfType(person, typeof(Odilon));
		}

		[TestMethod]
		public void AbstractContructorParameter_Test() {

			IocFactory.Register(Component.For<IMyComponent>().ImplementedBy<MyComponent>());

			IMyComponent myComponent = IocFactory.Resolve<IMyComponent>(new Dependency());
		}

		[TestMethod]
		public void MultipleParametersConstructors_Test() {

			IocFactory.Reset();

			IocFactory.Register(
				Component.For<IMultipleParametersConstructor>().ImplementedBy<MultipleParametersConstructor>()
				);

			IMultipleParametersConstructor constructor = IocFactory.Resolve<IMultipleParametersConstructor>(string.Empty, string.Empty, 66, "Banana");

			Assert.IsNotNull(constructor);
			Assert.AreEqual("Banana", constructor.Fruta);
		}

		[TestMethod]
		public void UserTypeAndPrimitiveTypeConstructor_Test() {

			IocFactory.Reset();

			IocFactory.Register(
				Component.For<IMultipleParametersConstructor>().ImplementedBy<MultipleParametersConstructor>()
				);

			IUserPrincipal userPrincipal = new UserPrincipal();
			userPrincipal.Login = "banana";

			string accessToken = Guid.NewGuid().ToString();

			IMultipleParametersConstructor constructor = IocFactory.Resolve<IMultipleParametersConstructor>(userPrincipal, accessToken);

			Assert.IsNotNull(constructor);
			Assert.AreEqual("banana", userPrincipal.Login);
		}

		[TestMethod]
		public void ResolveWithoutConcrete_Test() {

			IocFactory.Reset();

			IocFactory.Register(
				Component.For<IMultipleParametersConstructor>().ImplementedBy<MultipleParametersConstructor>()
				);

			IMultipleParametersConstructor person = IocFactory.Resolve<IMultipleParametersConstructor>();
		}
	}

	public interface IPerson {

		int Age { get; set; }

		string GetName();
	}

	public sealed class Pedro : IPerson {

		public int Age { get; set; }

		public string GetName() {
			return "Pedro";
		}
	}

	public sealed class Guilherme : IPerson {

		public int Age { get; set; }

		public string GetName() {
			return "Guilherme";
		}
	}

	public sealed class Odilon : IPerson {

		public int Age { get; set; }

		public string GetName() {
			return "Odilon";
		}
	}

	public interface IMyComponent {

	}

	public class MyComponent : IMyComponent {
		public MyComponent(BaseDependency dependency) { }
	}

	public abstract class BaseDependency { }

	public class Dependency : BaseDependency { }

	public interface IMultipleParametersConstructor {

		string Name { get; set; }

		string Email { get; set; }

		string Fruta { get; set; }

		int Age { get; set; }
	}

	public sealed class MultipleParametersConstructor : IMultipleParametersConstructor {

		public MultipleParametersConstructor(string name, string email, int age, string fruta) {
			this.Name = name;
			this.Email = email;
			this.Fruta = fruta;
			this.Age = age;
		}

		public MultipleParametersConstructor(IUserPrincipal userPrincipal, string token) {
			this.User = userPrincipal;
			this.Token = token;
		}

		public string Token { get; set; }

		public IUserPrincipal User { get; set; }

		public string Name { get; set; }

		public string Email { get; set; }

		public string Fruta { get; set; }

		public int Age { get; set; }
	}

	public sealed class UserPrincipal : IUserPrincipal {

		public UserPrincipal() { }

		public string Login { get; set; }
	}

	public interface IUserPrincipal {

		string Login { get; set; }
	}
}
