using Dlp.Authenticator.Utility;

namespace Dlp.Sdk.Tests.Authenticator.Mocks {

    public sealed class ConfigurationUtilityMock : IConfigurationUtility {

        public ConfigurationUtilityMock() { }

        public string GlobalIdentityHostAddress { get; set; }
    }
}
