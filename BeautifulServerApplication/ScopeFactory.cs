using Microsoft.Extensions.DependencyInjection;

namespace BeautifulServerApplication
{
	public interface IScopeFactory
	{
		IServiceScope Create();
	}

	internal class ScopeFactory(IServiceProvider serviceProvider) : IScopeFactory
	{
		public IServiceScope Create()
		{
			return serviceProvider.CreateScope();
		}
	}
}