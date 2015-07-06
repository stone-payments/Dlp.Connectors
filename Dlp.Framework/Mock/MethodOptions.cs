using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Framework.Mock {

    internal sealed class MethodOptions<R> : IMethodOptions<R> {

        internal MethodOptions() { }

		public string MemberFullName { get; set; }

		public string MethodName { get; set; }

		public object ReturnValue { get; set; }

        public IMethodOptions<R> Return(R returnValue) {

			this.ReturnValue = returnValue;

            return this;
        }
    }
}
