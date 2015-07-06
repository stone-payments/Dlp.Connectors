using Dlp.Framework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.Serialization;

namespace Dlp.Sdk.Tests.Framework {

    [ExcludeFromCodeCoverage]
    [DataContract]
    public sealed class ValidateClientApiRequest {

        public ValidateClientApiRequest() { }

        [DataMember]
        public string RawData { get; set; }

        [DataMember]
        public string EncryptedData { get; set; }

        [DataMember]
        public Guid ApplicationKey { get; set; }

        [DataMember]
        public Guid ClientApplicationKey { get; set; }
    }

    [ExcludeFromCodeCoverage]
    [DataContract]
    public sealed class ValidateClientApiResponse {

        public ValidateClientApiResponse() { }

        [DataMember]
        public bool Success { get; set; }

        [DataMember]
        public List<ApiReport> OperationReport { get; set; }
    }

    [ExcludeFromCodeCoverage]
    [DataContract]
    public sealed class ApiReport {

        public ApiReport() { }

        [DataMember]
        public string Field { get; set; }

        [DataMember]
        public string Message { get; set; }
    }

    [ExcludeFromCodeCoverage]
    [TestClass]
    public class RestClientTest {

        [TestMethod]
        public void SubmitGetJsonSuccessTest() {

            string endpoint = "http://b4riodsk-028/GlobalIdentity/api/Authorization/GetApplicationName/569CC6EB-1B2C-4056-BEAB-A369BAD33385";

            WebResponse<string> result = RestClient.SendHttpWebRequest<string>(null, HttpVerb.Get, HttpContentType.Json, endpoint, null);

            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        public void SubmitGetXmlSuccessTest() {

            string endpoint = "http://b4riodsk-028/GlobalIdentity/api/Authorization/GetApplicationName/569CC6EB-1B2C-4056-BEAB-A369BAD33385";

            WebResponse<string> result = RestClient.SendHttpWebRequest<string>(null, HttpVerb.Get, HttpContentType.Xml, endpoint, null);

            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);
        }

        [TestMethod]
        public void SubmitPostJsonWithComplexResponseSuccessTest() {

            string endpoint = "http://b4riodsk-028/GlobalIdentity/api/Authorization/ValidateApplication";

            ValidateClientApiRequest request = new ValidateClientApiRequest() {
                RawData = "banana_1234",
                EncryptedData = "22CEFD519969617EAEB9E9A327BB1369BD9D86B2BC782DEC6CE7A8763BD5D11B274F12B4CBB3E7FA8128AD9DC356152D7A0D922FE32D8B634843D48D21112CD3",
                ApplicationKey = Guid.Parse("569CC6EB-1B2C-4056-BEAB-A369BAD33385"),
                ClientApplicationKey = Guid.Parse("6a3204c5-6f5f-4d8d-a9e9-ddcf53a364dd")
            };

            WebResponse<ValidateClientApiResponse> result = RestClient.SendHttpWebRequest<ValidateClientApiResponse>(request, HttpVerb.Post, HttpContentType.Json, endpoint, null);

            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);
            Assert.IsTrue(result.ResponseData.Success);
        }

        [TestMethod]
        public void SubmitPostJsonWithStringResponseSuccessTest() {

            string endpoint = "http://b4riodsk-028/GlobalIdentity/api/Authorization/ValidateApplication";

            ValidateClientApiRequest request = new ValidateClientApiRequest() {
                //RawData = "banana_1234",
                //EncryptedData = "22CEFD519969617EAEB9E9A327BB1369BD9D86B2BC782DEC6CE7A8763BD5D11B274F12B4CBB3E7FA8128AD9DC356152D7A0D922FE32D8B634843D48D21112CD3",
                //ApplicationKey = Guid.Parse("569CC6EB-1B2C-4056-BEAB-A369BAD33385")
                RawData = "25.004.658/0001-00-Test NET.TCPba1d",
                EncryptedData = "6AB8F0B186C037BC48CA16BEC9CD34A453479997466CF166A0C1501E45F785D66E7C5F049E525B62C62D02867BC1C42366345EDCBB3C01FFC4EA039B23F858C6",
                ApplicationKey = Guid.Parse("569cc6eb-1b2c-4056-beab-a369bad33385"),
                ClientApplicationKey = Guid.Parse("6a3204c5-6f5f-4d8d-a9e9-ddcf53a364dd")
            };

            WebResponse<string> result = RestClient.SendHttpWebRequest<string>(request, HttpVerb.Post, HttpContentType.Json, endpoint, null);

            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(result.ResponseData, "{\"Success\":true,\"OperationReport\":[]}");
        }

        [TestMethod]
        public void SubmitPostXmlWithComplexResponseSuccessTest() {

            string endpoint = "http://b4riodsk-028/GlobalIdentity/api/Authorization/ValidateApplication";

            ValidateClientApiRequest request = new ValidateClientApiRequest() {
                RawData = "banana_1234",
                EncryptedData = "22CEFD519969617EAEB9E9A327BB1369BD9D86B2BC782DEC6CE7A8763BD5D11B274F12B4CBB3E7FA8128AD9DC356152D7A0D922FE32D8B634843D48D21112CD3",
                ApplicationKey = Guid.Parse("569CC6EB-1B2C-4056-BEAB-A369BAD33385"),
                ClientApplicationKey = Guid.Parse("6a3204c5-6f5f-4d8d-a9e9-ddcf53a364dd")
            };

            WebResponse<ValidateClientApiResponse> result = RestClient.SendHttpWebRequest<ValidateClientApiResponse>(request, HttpVerb.Post, HttpContentType.Xml, endpoint, null);

            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);
            Assert.IsTrue(result.ResponseData.Success);
        }

        [TestMethod]
        public void SubmitPostXmlWithStringResponseSuccessTest() {

            string endpoint = "http://b4riodsk-028/GlobalIdentity/api/Authorization/ValidateApplication";

            ValidateClientApiRequest request = new ValidateClientApiRequest() {
                RawData = "banana_1234",
                EncryptedData = "22CEFD519969617EAEB9E9A327BB1369BD9D86B2BC782DEC6CE7A8763BD5D11B274F12B4CBB3E7FA8128AD9DC356152D7A0D922FE32D8B634843D48D21112CD3",
                ApplicationKey = Guid.Parse("569CC6EB-1B2C-4056-BEAB-A369BAD33385"),
                ClientApplicationKey = Guid.Parse("6a3204c5-6f5f-4d8d-a9e9-ddcf53a364dd")
            };

            WebResponse<string> result = RestClient.SendHttpWebRequest<string>(request, HttpVerb.Post, HttpContentType.Xml, endpoint, null);

            Assert.AreEqual(result.StatusCode, HttpStatusCode.OK);
            Assert.AreEqual(result.ResponseData, "<ValidateClientApiResponse xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"><Success>true</Success><OperationReport /></ValidateClientApiResponse>");
        }

        [TestMethod]
        public void CheckValidCepTest() {

            string endpoint = "http://viacep.com.br/ws/01001-000/json/";

            WebResponse<string> result = RestClient.SendHttpWebRequest<string>(null, HttpVerb.Get, HttpContentType.Json, endpoint, null);
        }
    }
}
