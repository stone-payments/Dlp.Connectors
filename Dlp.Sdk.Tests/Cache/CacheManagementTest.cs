using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using Dlp.Framework;

namespace Dlp.Sdk.Tests.Cache {

    [TestClass]
    public class CacheManagementTest {

        [TestMethod]
        public void GetByKey_NullEntry_Test() {

            //CacheManagement cacheManagement = new CacheManagement();

            //MerchantData merchantData = new MerchantData();

            //merchantData.MerchantId = 2;
            //merchantData.MerchantName = "Loja de teste";

            //string data = Serializer.JsonSerialize(merchantData);

            //CacheManagement.Set(data, "MerchantData", typeof(MerchantData).FullName, "Teste", 60);

            //string result = CacheManagement.GetByKey("MerchantData2", typeof(MerchantData).FullName, "Teste");
        }
    }

    public sealed class MerchantData {

        public MerchantData() { }

        public string MerchantName { get; set; }

        public int MerchantId { get; set; }
    }
}
