using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dlp.Connectors;

namespace Dlp.Sdk.Tests.Connectors {

    [TestClass]
    public class CacheConnectorTest {

        [TestMethod]
        public void Connect() {

            bool connected = false;
            CacheConnector connector = null;

            try {
                connector = new CacheConnector(Guid.Parse("{A83B553F-11D5-4689-85A6-3BFDA90354C5}"), "dlpcache.cloudapp.net", 10100);

                connected = connector.Connect();
            }
            finally {

                if (connected == true) { connector.Disconnect(); }
            }
        }

        [TestMethod]
        public void CacheConnector_AddItem_Test() {

            MerchantData expected = new MerchantData() { MerchantId = 2, MerchantName = "Loja de teste" };
            MerchantData result = null;
            bool connected = false;

            CacheConnector connector = null;

            try {
                //connector = new CacheConnector(Guid.Parse("{A83B553F-11D5-4689-85A6-3BFDA90354C5}"), "dlpcache.cloudapp.net", 10100);
                connector = new CacheConnector(Guid.Parse("{A83B553F-11D5-4689-85A6-3BFDA90354C5}"), "jonssen-pc", 6771);

                connected = connector.Connect();

                if (connected == true) {

                    result = connector.GetByKey<MerchantData>("MerchantName");

                    MerchantData merchantData = new MerchantData();

                    merchantData.MerchantId = 2;
                    merchantData.MerchantName = "Loja de teste";

                    bool inserted = connector.Set<MerchantData>("MerchantName", merchantData, 10);

                    connector.Disconnect();

                    connector.Connect();

                    if (inserted == true) {

                        result = connector.GetByKey<MerchantData>("MerchantName");
                    }
                }
            }
            finally {

                if (connected == true) { connector.Disconnect(); }
            }

            Assert.IsNotNull(result);
            Assert.AreEqual(expected.MerchantId, result.MerchantId);
            Assert.AreEqual(expected.MerchantName, result.MerchantName);
        }

        public sealed class MerchantData {

            public MerchantData() { }

            public string MerchantName { get; set; }

            public int MerchantId { get; set; }
        }
    }
}
