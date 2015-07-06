using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dlp.Authenticator.DataContracts;
using Dlp.Framework;
using Dlp.Authenticator.Utility;
using Dlp.Sdk.Tests.Authenticator.Mocks;
using Dlp.Framework.Container;

namespace Dlp.Sdk.Tests.Authenticator {

    [TestClass]
    public class AuthenticatorTest {

        private Guid TEST_APPLICATION_KEY = Guid.Parse("6f3ef834-446f-48d7-9cfb-5a85a8a2576a");
        private Guid TEST_CLIENT_APPLICATION_KEY = Guid.Parse("2d8299ce-ae50-4c77-bf69-c436ed7650a7");
        private const string TEST_CLIENT_APPLICATION_SECRET_KEY = "DA7B4F298A92F94F02574594CAAF4CB16A83BCE0";
        private const string TEST_ACCOUNT_LOGIN = "teste@stone.com.br";
        private const string TEST_ACCOUNT_PASSWORD = "123Mudar...";
        private Guid TEST_USER_KEY = Guid.Parse("078d59eb-ae2c-46c2-a995-b8b5c21fcde8");
        private const string TEST_USER_NAME = "Usuário de teste";

        [ClassInitialize]
        public static void ClassInitialize(TestContext context) {

            ConfigurationUtilityMock configurationUtility = new ConfigurationUtilityMock();

            configurationUtility.GlobalIdentityHostAddress = "https://dlpgi.dlp-payments.com/";

            IocFactory.Register(
                Component.For<IConfigurationUtility>().Instance(configurationUtility)
                );
        }

        [TestMethod]
        public void AuthenticateUser_Test() {

            AuthenticateUserRequest request = new AuthenticateUserRequest();

            request.ApplicationKey = TEST_APPLICATION_KEY;
            request.Email = TEST_ACCOUNT_LOGIN;
            request.Password = TEST_ACCOUNT_PASSWORD;

            Dlp.Authenticator.GlobalIdentity globalIdentity = new Dlp.Authenticator.GlobalIdentity();

            AuthenticateUserResponse response = globalIdentity.AuthenticateUser(request);

            Assert.IsNotNull(response);
            Assert.IsTrue(response.Success);
            Assert.AreEqual(TEST_USER_NAME, response.Name);
            Assert.AreEqual(TEST_USER_KEY, response.UserKey);
            Assert.IsNotNull(response.AuthenticationToken);
        }

        [TestMethod]
        public void AuthenticateUser_InvalidPassword_Test() {

            AuthenticateUserRequest request = new AuthenticateUserRequest();

            request.ApplicationKey = TEST_APPLICATION_KEY;
            request.Email = TEST_ACCOUNT_LOGIN;
            request.Password = "senhaerrada";

            Dlp.Authenticator.GlobalIdentity globalIdentity = new Dlp.Authenticator.GlobalIdentity();

            AuthenticateUserResponse response = globalIdentity.AuthenticateUser(request);

            Assert.IsNotNull(response);
            Assert.IsFalse(response.Success);
            Assert.IsNull(response.Name);
            Assert.IsNull(response.AuthenticationToken);
            Assert.AreEqual(Guid.Empty, response.UserKey);
            Assert.IsNotNull(response.OperationReport);
            Assert.AreEqual(1, response.OperationReport.Count);
        }

        [TestMethod]
        public void ValidateToken_Test() {

            AuthenticateUserRequest authenticateUserRequest = new AuthenticateUserRequest();

            authenticateUserRequest.ApplicationKey = TEST_APPLICATION_KEY;
            authenticateUserRequest.Email = TEST_ACCOUNT_LOGIN;
            authenticateUserRequest.Password = TEST_ACCOUNT_PASSWORD;

            Dlp.Authenticator.GlobalIdentity globalIdentity = new Dlp.Authenticator.GlobalIdentity();

            AuthenticateUserResponse authenticateUserResponse = globalIdentity.AuthenticateUser(authenticateUserRequest);

            Assert.IsNotNull(authenticateUserResponse);
            Assert.IsTrue(authenticateUserResponse.Success);
            Assert.IsNotNull(authenticateUserResponse.AuthenticationToken);

            ValidateTokenRequest validateTokenRequest = new ValidateTokenRequest();

            validateTokenRequest.ApplicationKey = TEST_APPLICATION_KEY;
            validateTokenRequest.Token = authenticateUserResponse.AuthenticationToken;

            ValidateTokenResponse validateTokenResponse = globalIdentity.ValidateToken(validateTokenRequest);

            Assert.IsNotNull(validateTokenResponse);
            Assert.IsTrue(validateTokenResponse.Success);
            Assert.IsTrue(validateTokenResponse.ExpirationInMinutes > 0);
        }

        [TestMethod]
        public void ValidateToken_InvalidToken_Test() {

            ValidateTokenRequest validateTokenRequest = new ValidateTokenRequest();

            validateTokenRequest.ApplicationKey = TEST_APPLICATION_KEY;
            validateTokenRequest.Token = "Invalid Token";

            Dlp.Authenticator.GlobalIdentity globalIdentity = new Dlp.Authenticator.GlobalIdentity();

            ValidateTokenResponse validateTokenResponse = globalIdentity.ValidateToken(validateTokenRequest);

            Assert.IsNotNull(validateTokenResponse);
            Assert.IsFalse(validateTokenResponse.Success);
            Assert.AreEqual(0, validateTokenResponse.ExpirationInMinutes);
            Assert.IsNotNull(validateTokenResponse.OperationReport);
            Assert.AreEqual(1, validateTokenResponse.OperationReport.Count);
        }

        [TestMethod]
        public void IsUserInRole_Test() {

            IsUserInRoleRequest isUserInRoleRequest = new IsUserInRoleRequest();

            isUserInRoleRequest.ApplicationKey = TEST_APPLICATION_KEY;
            isUserInRoleRequest.UserKey = TEST_USER_KEY;
            isUserInRoleRequest.RoleCollection.Add("User");

            Dlp.Authenticator.GlobalIdentity globalIdentity = new Dlp.Authenticator.GlobalIdentity();

            IsUserInRoleResponse isUserInRoleResponse = globalIdentity.IsUserInRole(isUserInRoleRequest);

            Assert.IsNotNull(isUserInRoleResponse);
            Assert.IsTrue(isUserInRoleResponse.Success);
        }

        [TestMethod]
        public void IsUserInRole_InvalidRole_Test() {

            IsUserInRoleRequest isUserInRoleRequest = new IsUserInRoleRequest();

            isUserInRoleRequest.ApplicationKey = TEST_APPLICATION_KEY;
            isUserInRoleRequest.UserKey = TEST_USER_KEY;
            isUserInRoleRequest.RoleCollection.Add("Banana");

            Dlp.Authenticator.GlobalIdentity globalIdentity = new Dlp.Authenticator.GlobalIdentity();

            IsUserInRoleResponse isUserInRoleResponse = globalIdentity.IsUserInRole(isUserInRoleRequest);

            Assert.IsNotNull(isUserInRoleResponse);
            Assert.IsFalse(isUserInRoleResponse.Success);
        }

        [TestMethod]
        public void ValidateApplication_Test() {

            ValidateApplicationRequest validateApplicationRequest = new ValidateApplicationRequest();

            validateApplicationRequest.ApplicationKey = TEST_APPLICATION_KEY;
            validateApplicationRequest.ClientApplicationKey = TEST_CLIENT_APPLICATION_KEY;
            validateApplicationRequest.RawData = "Banana";
            validateApplicationRequest.EncryptedData = "Banana".CalculateSha512(TEST_CLIENT_APPLICATION_SECRET_KEY);

            Dlp.Authenticator.GlobalIdentity globalIdentity = new Dlp.Authenticator.GlobalIdentity();

            ValidateApplicationResponse validateApplicationResponse = globalIdentity.ValidateApplication(validateApplicationRequest);

            Assert.IsNotNull(validateApplicationResponse);
            Assert.IsTrue(validateApplicationResponse.Success);
        }

        [TestMethod]
        public void ValidateApplication_InvalidSecretKey_Test() {

            ValidateApplicationRequest validateApplicationRequest = new ValidateApplicationRequest();

            validateApplicationRequest.ApplicationKey = TEST_APPLICATION_KEY;
            validateApplicationRequest.ClientApplicationKey = TEST_CLIENT_APPLICATION_KEY;
            validateApplicationRequest.RawData = "Banana";
            validateApplicationRequest.EncryptedData = "Banana".CalculateSha512("InvalidKey");

            Dlp.Authenticator.GlobalIdentity globalIdentity = new Dlp.Authenticator.GlobalIdentity();

            ValidateApplicationResponse validateApplicationResponse = globalIdentity.ValidateApplication(validateApplicationRequest);

            Assert.IsNotNull(validateApplicationResponse);
            Assert.IsFalse(validateApplicationResponse.Success);
            Assert.IsNotNull(validateApplicationResponse.OperationReport);
            Assert.AreEqual(0, validateApplicationResponse.OperationReport.Count);
        }
    }
}
