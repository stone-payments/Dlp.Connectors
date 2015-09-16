using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Sdk.Tests.Mocks {

    public interface IConfigurationUtilityMock {

        string TestValue { get; set; }

        string GetName(string originalName);
    }
}
