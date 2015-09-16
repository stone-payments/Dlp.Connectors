using Dlp.Framework.Container;
using Dlp.Framework.Container.Interceptors;
using Dlp.Framework.Mock;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Sdk.Tests {

	[TestClass]
	public class MockerTest {

		[TestMethod]
		public void CreateMock_Test() {

			#region Temporário

			//IocFactory.Register(
			//	Component.For<IMockerSimulation>().ImplementedBy<MockerSimulation>()
			//	.Interceptor<MockerSimulationInterceptor>()
			//	);

			//IMockerSimulation mockerSimulation = IocFactory.Resolve<IMockerSimulation>();

			//long mockerAge = mockerSimulation.GetAge<long>(25, "Banana");
			//int mockerAnotherAge = mockerSimulation.GetAnotherAge<int>(12);

			#endregion

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
		public void MyTestMethod() {

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

		[TestMethod]
		public void CreateMethod() {

			/**
			 * Gera um assembly com o seguinte método:
			 * 
			 *  public static R GenericMethod<T, R>(T[] tarray) where R : class, ICollection<T>, new() {
			 * 
			 *		T ret = new T();
			 *		ICollection<R> ic = ret;
			 *
			 *		foreach (R t in tarray)
			 *		{
			 *			ic.Add(t);
			 *		}
			 *		return ret;
			 *	}
			 **/

			AssemblyName assemblyName = new AssemblyName("MethodGenerator");
			AppDomain domain = AppDomain.CurrentDomain;
			AssemblyBuilder assemblyBuilder = domain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);

			ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name, assemblyName + ".dll");

			TypeBuilder typeBuilder = moduleBuilder.DefineType("GenericType", TypeAttributes.Public);

			MethodBuilder methodBuilder = typeBuilder.DefineMethod("GenericMethod", MethodAttributes.Public | MethodAttributes.Static);

			string[] typeParameterNames = { "T", "R" };
			GenericTypeParameterBuilder[] typeParameters = methodBuilder.DefineGenericParameters(typeParameterNames);

			GenericTypeParameterBuilder T = typeParameters[0];
			GenericTypeParameterBuilder R = typeParameters[1];

			// Define os atributos do tipo genérico de resposta.
			R.SetGenericParameterAttributes(GenericParameterAttributes.ReferenceTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint);

			// Define as constraints para o tipo genérico de resposta.
			Type icoll = typeof(ICollection<>);
			Type icollOfInput = icoll.MakeGenericType(T);
			Type[] constraints = { icollOfInput };
			R.SetInterfaceConstraints(constraints);

			// Define os parâmetros do método.
			Type[] parms = { T.MakeArrayType() };
			methodBuilder.SetParameters(parms);

			// Define o tipo de retorno do método.
			methodBuilder.SetReturnType(R);

			#region IL
			// Gera o método com ILGenerator.
			ILGenerator ilGenerator = methodBuilder.GetILGenerator();

			// Cria as variáveis locais.
			LocalBuilder retVal = ilGenerator.DeclareLocal(R);
			LocalBuilder ic = ilGenerator.DeclareLocal(icollOfInput);
			LocalBuilder input = ilGenerator.DeclareLocal(T.MakeArrayType());
			LocalBuilder index = ilGenerator.DeclareLocal(typeof(int));

			Label enterLoop = ilGenerator.DefineLabel();
			Label loopAgain = ilGenerator.DefineLabel();

			// Carrega o parâmetro recebido no método.
			ilGenerator.Emit(OpCodes.Ldarg_0);

			// Armazena na variável local input.
			ilGenerator.Emit(OpCodes.Stloc_S, input);

			MethodInfo createInstanceMethodInfo = typeof(Activator).GetMethod("CreateInstance", Type.EmptyTypes);
			MethodInfo outputMethodInfo = createInstanceMethodInfo.MakeGenericMethod(R);

			// Gera o código para criar uma instancia do objeto a ser retornado (R ret = new R()).
			ilGenerator.Emit(OpCodes.Call, outputMethodInfo);
			ilGenerator.Emit(OpCodes.Stloc_S, retVal);

			// Gera o código para criar uma instancia que conterá a coleção com os parâmetros recebidos (ICollection<R> ic = ret). Faz o cast do array para ICollection<R>.
			ilGenerator.Emit(OpCodes.Ldloc_S, retVal);
			ilGenerator.Emit(OpCodes.Box, R);
			ilGenerator.Emit(OpCodes.Castclass, icollOfInput);
			ilGenerator.Emit(OpCodes.Stloc_S, ic);

			// Obtém o método Add específico para o tipo genérico.
			MethodInfo mAddPrep = icoll.GetMethod("Add");
			MethodInfo mAdd = TypeBuilder.GetMethod(icollOfInput, mAddPrep);

			// Prepara o loop.
			ilGenerator.Emit(OpCodes.Ldc_I4_0);
			ilGenerator.Emit(OpCodes.Stloc_S, index);
			ilGenerator.Emit(OpCodes.Br_S, enterLoop);

			// Gera o código do corpo do loop.
			ilGenerator.MarkLabel(loopAgain);

			// Carrega a coleção a ser retornada.
			ilGenerator.Emit(OpCodes.Ldloc_S, ic);
			// Carrega os dados recebidos no parâmetro.
			ilGenerator.Emit(OpCodes.Ldloc_S, input);
			// Carrega o contador do loop.
			ilGenerator.Emit(OpCodes.Ldloc_S, index);
			// Utiliza os dois valores carregados anteriormente (array e índice) para obter o elemento do tipo T. Input e index são removidos do stack e o elemento é adicionado no lugar.
			ilGenerator.Emit(OpCodes.Ldelem, T);
			// Adiciona o elemento na lista a ser retornada. O ic que continua no stack será a lista utilizada e o elemento adicionado acima serão utilzados e removidos do stack.
			ilGenerator.Emit(OpCodes.Callvirt, mAdd);

			// Carrega o contador no stack.
			ilGenerator.Emit(OpCodes.Ldloc_S, index);
			// Carrega o valor 1 no stack, que será utilizado para incrementar o contador.
			ilGenerator.Emit(OpCodes.Ldc_I4_1);
			// Incrementa o contador e armazena o resultado no stack.
			ilGenerator.Emit(OpCodes.Add);
			// Armazena o valor incrementado como novo valor do contador.
			ilGenerator.Emit(OpCodes.Stloc_S, index);

			// Define o ponto inicial do loop.
			ilGenerator.MarkLabel(enterLoop);

			// Carrega o contador no stack.
			ilGenerator.Emit(OpCodes.Ldloc_S, index);
			// Carrega os parâmetros no stack.
			ilGenerator.Emit(OpCodes.Ldloc_S, input);
			// Armazena o tamanho do array (input) no stack e remove o input do stack.
			ilGenerator.Emit(OpCodes.Ldlen);
			// Converte o tamanho do array encontrado para inteiro e remove Ldlen do stack.
			ilGenerator.Emit(OpCodes.Conv_I4);
			// Compara o index e o tamanho do array. Ambos os itens serão removidos do stack. Caso o index for menor que o tamanho do array, retorna 1. Caso contrário retorna 0.
			ilGenerator.Emit(OpCodes.Clt);
			// Caso o index seja menor que o tamanho do array (retornou 1), (re)inicia o loop.
			ilGenerator.Emit(OpCodes.Brtrue_S, loopAgain);

			// Armazana o resultado do loop e gera a linha de retorno (return ret)
			ilGenerator.Emit(OpCodes.Ldloc_S, retVal);
			ilGenerator.Emit(OpCodes.Ret);

			#endregion

			// Cria o tipo de salva a dll.
			Type generatedType = typeBuilder.CreateType();
			assemblyBuilder.Save(assemblyName.Name + ".dll");
		}
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
