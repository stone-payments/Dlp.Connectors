using Dlp.Framework.Container.Interceptors;
using Dlp.Framework.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;

namespace Dlp.Sdk.Tests {

	[TestClass]
	public class MockerTest {

		[TestMethod]
		public void CreateMock_Test() {

			Guid applicationKey = Guid.NewGuid();

			IMockerSimulation mock = Mocker.CreateMock<IMockerSimulation>();

			BasicRequest request = new BasicRequest();
			request.Id = 77;

			mock.Stub(p => p.GetGender()).Return(77);
			mock.Stub(p => p.GetAge<short>(12, "Banana")).Return(15);
			mock.Stub(p => p.GetAnotherAge<short>(10)).Return(13);
			mock.Stub(p => p.ActualName<BasicRequest>(request)).Return(true);
			mock.Stub(p => p.ValidateName("Banana")).Return(true);
			mock.Stub(p => p.GetApplicationId(applicationKey)).Return(2);
			mock.Stub(p => p.GetNameById(Convert.ToInt32("2"))).Return("Banana");
			mock.Stub(p => p.GetNameById(10)).Return("Cebola");
			mock.Stub(x => x.GetValue()).Return(true);
			mock.Stub(p => p.Name).Return("Teste de mock");
			mock.Stub(p => p.GetStatusName("enabled".Equals("enabled"))).Return("Enabled");

			MethodInfo[] miList = mock.GetType().GetMethods();

			BasicRequest testRequest = new BasicRequest();
			testRequest.Id = 76;

			byte gender = mock.GetGender();
			long age = mock.GetAge<short>(12, "Banana");
			short anotherAge = mock.GetAnotherAge<short>(10);
			bool actualName = mock.ActualName<BasicRequest>(testRequest);
			bool validatedName = mock.ValidateName("Banana");
			string name = mock.GetNameById(2);
			bool val = mock.GetValue();
			string nameProperty = mock.Name;
			string statusName = mock.GetStatusName(false);

			Assert.IsTrue(validatedName);
			Assert.AreEqual("Banana", name);
			Assert.IsTrue(val);
			Assert.AreEqual("Teste de mock", nameProperty);
			Assert.IsNull(mock.GetNameById(1));
		}

		[TestMethod]
		public void MultipleMocks_Test() {

			IRepository<MerchantEntity> merchantEntityMock = Mocker.CreateMock<IRepository<MerchantEntity>>();

			MerchantEntity merchantEntity = new MerchantEntity();
			merchantEntity.MerchantName = "Mocked Merchant";

			BasicRequest request = new BasicRequest();
			request.Id = 2;

			merchantEntityMock.Stub(p => p.GetById(request)).Return(merchantEntity);

			BasicRequest requestTest = new BasicRequest();
			requestTest.Id = 2;

			MerchantEntity merchantResult = merchantEntityMock.GetById(requestTest);

			Assert.IsNotNull(merchantResult);
			Assert.AreEqual("Mocked Merchant", merchantResult.MerchantName);

			IRepository<AcquirerEntity> acquirerEntityMock = Mocker.CreateMock<IRepository<AcquirerEntity>>();

			AcquirerEntity acquirerEntity = new AcquirerEntity();
			acquirerEntity.AcquirerName = "Mocked Acquirer";

			BasicRequest acquirerRequest = new BasicRequest();
			acquirerRequest.Id = 2;

			acquirerEntityMock.Stub(p => p.GetById(acquirerRequest)).Return(acquirerEntity);

			BasicRequest acquirerRequestTest = new BasicRequest();
			acquirerRequestTest.Id = 2;

			AcquirerEntity acquirerResult = acquirerEntityMock.GetById(acquirerRequestTest);

			Assert.IsNotNull(acquirerResult);
			Assert.AreEqual("Mocked Acquirer", acquirerResult.AcquirerName);

			AcquirerEntity anotherAquirerEntity = new AcquirerEntity();
			anotherAquirerEntity.AcquirerName = "Another Mocked Acquirer";

			BasicRequest acquirerRequest2 = new BasicRequest();
			acquirerRequest2.Id = 3;

			acquirerEntityMock.Stub(p => p.GetById(acquirerRequest2)).Return(anotherAquirerEntity);
			acquirerEntityMock.Stub(p => p.Name).Return("Banana");
			acquirerEntityMock.Stub(p => p.GetTypeName()).Return(new AcquirerEntity() { AcquirerName = "Mocked Acquirer Type" });

			BasicRequest anotherAcquirerRequestTest = new BasicRequest();
			anotherAcquirerRequestTest.Id = 3;

			AcquirerEntity anotherAcquirerResult = acquirerEntityMock.GetById(anotherAcquirerRequestTest);

			Assert.IsNotNull(anotherAcquirerResult);
			Assert.AreEqual("Another Mocked Acquirer", anotherAcquirerResult.AcquirerName);

			string nameResult = acquirerEntityMock.Name;

			Assert.IsNotNull(nameResult);
			Assert.AreEqual("Banana", nameResult);

			AcquirerEntity mockAcquirerType = acquirerEntityMock.GetTypeName();

			Assert.IsNotNull(mockAcquirerType);
			Assert.AreEqual("Mocked Acquirer Type", mockAcquirerType.AcquirerName);
		}

		#region Types

		public abstract class AbstractRequest {

			public int Id { get; set; }
		}

		public sealed class BasicRequest : AbstractRequest {

			public BasicRequest() { }
		}

		public sealed class MerchantRepository<T> : IRepository<T> where T : IEntity {

			public MerchantRepository() { }

			public T GetById(AbstractRequest request) {
				return default(T);
			}

			public string Name { get; set; }

			public T GetTypeName() {
				return default(T);
			}
		}

		public sealed class AcquirerRepository<T> : IRepository<T> where T : IEntity {

			public AcquirerRepository() { }

			public T GetById(AbstractRequest request) {
				return default(T);
			}

			public string Name { get; set; }

			public T GetTypeName() {
				return default(T);
			}
		}

		public interface IRepository<T> where T : IEntity {

			T GetById(AbstractRequest request);

			string Name { get; set; }

			T GetTypeName();
		}

		public interface IEntity { }

		public sealed class MerchantEntity : IEntity {

			public MerchantEntity() { }

			public string MerchantName { get; set; }
		}

		public sealed class AcquirerEntity : IEntity {

			public AcquirerEntity() { }

			public string AcquirerName { get; set; }
		}

		#endregion
	}

	public interface IMockerSimulation {

		string Name { get; set; }

		bool GetValue();

		byte GetGender();

		string GetNameById(int Id);

		string GetStatusName(bool status);

		Nullable<int> GetApplicationId(Guid applicationKey);

		bool ValidateName(string name);

		long GetAge<T>(int count, string name);

		T GetAnotherAge<T>(int count);

		bool ActualName<T>(T data);
	}

	public class MockerSimulation : IMockerSimulation {

		public string Name { get; set; }

		public byte GetGender() {

			return 77;
		}

		public bool GetValue() {
			return true;
		}

		public string GetNameById(int Id) {
			return "Banana";
		}

		public string GetStatusName(bool status) {
			return "Banana";
		}

		public Nullable<int> GetApplicationId(Guid applicationKey) {
			return 2;
		}

		public bool ValidateName(string name) {
			return true;
		}

		public long GetAge<T>(int count, string name) {
			return count;
		}

		public T GetAnotherAge<T>(int count) {
			return default(T);
		}

		public bool ActualName<T>(T data) {
			return false;
		}
	}

	public class MockerSimulationInterceptor : IInterceptor {
		public void Intercept(IInvocation invocation) {
			invocation.Proceed();
		}
	}
}
