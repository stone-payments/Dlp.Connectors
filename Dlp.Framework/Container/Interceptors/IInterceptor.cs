
namespace Dlp.Framework.Container.Interceptors {

    public interface IInterceptor {

        void Intercept(IInvocation invocation);
    }
}
