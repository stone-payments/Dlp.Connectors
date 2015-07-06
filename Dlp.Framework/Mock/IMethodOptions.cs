namespace Dlp.Framework.Mock {

	public interface IMethodOptions {

		string MemberFullName { get; set; }

		string MethodName { get; set; }

		object ReturnValue { get; set; }
	}

    public interface IMethodOptions<R> : IMethodOptions {

        IMethodOptions<R> Return(R returnValue);
    }
}
