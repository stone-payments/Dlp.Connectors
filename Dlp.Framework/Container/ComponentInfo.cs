using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Framework.Container {

    public sealed class ComponentInfo : AbstractComponentInfo, IRegistration {

        public ComponentInfo() { }

        public IRegistrationInfo[] Register() {

            return new IRegistrationInfo[] { this };
        }
    }
}
