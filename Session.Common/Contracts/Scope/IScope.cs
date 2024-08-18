using Microsoft.Extensions.DependencyInjection;

namespace Session.Common.Contracts.Scope
{
	public interface IScope
	{
		string Id { get; }
		IServiceScope ServiceScope { get; }
		T GetService<T>() where T : class;
	}
}