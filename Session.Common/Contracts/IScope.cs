using Microsoft.Extensions.DependencyInjection;

namespace Session.Common.Contracts
{
	public interface IScope
	{
		string Id { get; }
		IServiceScope ServiceScope { get; }
		T GetService<T>() where T : class;
	}
}