using Microsoft.Extensions.DependencyInjection;

namespace Session.Common.Contracts.Services
{
	public interface IScopeFactory
	{
		IServiceScope Create();
	}
}