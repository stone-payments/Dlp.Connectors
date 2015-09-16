using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Sdk.Tests.Mocks {

    public sealed class ConfigurationUtilityMock : IConfigurationUtilityMock {

        public ConfigurationUtilityMock() { }

        public ConfigurationUtilityMock(string connectionString) { }

        public ConfigurationUtilityMock(DateTime agora) { }

        public string TestValue { get; set; }

        public string AnotherTest { get; set; }

        public string GetName(string originalName) { return originalName + " Mock 1"; }
    }
}
