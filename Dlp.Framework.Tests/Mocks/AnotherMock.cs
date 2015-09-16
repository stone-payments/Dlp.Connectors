using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Sdk.Tests.Mocks {

    public sealed class AnotherMock : IConfigurationUtilityMock {

        public string TestValue { get; set; }

        public string GetName(string originalName) { return originalName + " Mock 2"; }
    }
}
