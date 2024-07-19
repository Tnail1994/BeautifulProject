using Core.Helpers;
using DbManagement.Common.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DbManagement.Common.Implementations
{
	public abstract class BaseDbContext<T> : DbContext, IDbContext where T : class
	{
		private readonly IDbContextSettings _dbContextSettings;

		protected BaseDbContext(IDbContextSettings dbContextSettings)
		{
			Id = GuidIdCreator.CreateString();

			_dbContextSettings = dbContextSettings;
			TypeNameOfCollectionEntries = typeof(T).Name;
		}


		protected DbSet<T>? Set { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => optionsBuilder
			.UseNpgsql(
				$@"Server={_dbContextSettings.ServerAdresse};Port={_dbContextSettings.Port};UserId={_dbContextSettings.UserId};Password={_dbContextSettings.Password};Database={_dbContextSettings.DatabaseName};",
				options => options.EnableRetryOnFailure())
			.LogTo(Console.WriteLine, LogLevel.Information);

		public string Id { get; }
		public string TypeNameOfCollectionEntries { get; }

		public IEnumerable<object>? GetEntities()
		{
			return Set;
		}
	}
}