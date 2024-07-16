using Microsoft.Extensions.DependencyInjection;
using Session.Common.Contracts.Services;

namespace Session.Services
{
	public class ScopeFactory : IScopeFactory
	{
		private readonly IServiceProvider _serviceProvider;

		public ScopeFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public IServiceScope Create()
		{
			return _serviceProvider.CreateScope();
		}
	}
}