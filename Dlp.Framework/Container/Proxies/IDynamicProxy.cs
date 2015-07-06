using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dlp.Framework.Container.Proxies {

    public interface IDynamicProxy {

        object Invoke(object proxy, MethodInfo methodInfo, object[] parameters, Type[] genericArguments);
    }
}
