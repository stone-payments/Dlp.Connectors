using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dlp.Framework.Container.Proxies;

namespace Dlp.Sdk.Tests.Framework {

    [TestClass]
    public class ProxyFactoryTest {

        [TestMethod]
        public void CreateGenericProxy_Test() {

            Repository repository = new Repository();

            IRepository proxy = (IRepository)DynamicProxy.NewInstance(repository);

            Teste teste = new Teste();

            teste.Name = "Um teste";

            Teste result = proxy.Save<Teste>("Resposta");
        }
    }

    public class Teste {

        public Teste() { }

        public string Name { get; set; }
    }

    public interface IRepository {

        T Save<T>(string name) where T : class, new();
    }

    public sealed class Repository : IRepository {

        public Repository() { }
        public T Save<T>(string name) where T : class, new() {

            Teste teste = new Teste();

            teste.Name = name;

            return teste as T;
        }
    }
}
