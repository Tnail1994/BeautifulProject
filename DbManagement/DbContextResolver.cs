using DbManagement.Common.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace DbManagement
{
	public class DbContextResolver : IDbContextResolver
	{
		private readonly IEnumerable<IDbContext> _dbContexts;

		public DbContextResolver(IServiceProvider serviceProvider)
		{
			_dbContexts = serviceProvider.GetServices<IDbContext>();
		}

		public IEnumerable<IDbContext> Get()
		{
			return _dbContexts;
		}
	}
}