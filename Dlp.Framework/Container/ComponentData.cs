using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Framework.Container {

    public sealed class ComponentData {

        public ComponentData() { }

        public bool IsSingleton { get; set; }

        public bool IsDefault { get; set; }

        public Type ConcreteType { get; set; }

        public string Name { get; set; }

        internal object Instance { get; set; }
    }
}
