using Core.Extensions;
using Core.Helpers;
using DbManagement.Common.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DbManagement.Common.Implementations
{
	public abstract class BaseDbContext<T> : DbContext, IDbContext where T : EntityDto
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

		public IEnumerable<TDto> GetEntities<TDto>() where TDto : EntityDto
		{
			if (Set == null)
			{
				this.LogError($"Cannot get entities because Set is not initialized ", "server");
				return Enumerable.Empty<TDto>();
			}

			return Set.Cast<TDto>();
		}

		public void AddEntity<TDto>(TDto dto) where TDto : EntityDto
		{
			var entityDto = dto as T;

			if (entityDto == null)
			{
				this.LogWarning($"Cannot add because dto is not {typeof(T)}", "server");
				return;
			}

			if (Set == null)
			{
				this.LogError($"Cannot add because Set is not initialized ", "server");
				return;
			}

			if (!Set.Contains(entityDto))
			{
				this.LogDebug($"Adding type {typeof(T)}, with {entityDto}");
				Set.Add(entityDto);
			}

			SaveChanges();
		}

		public IEnumerable<object>? GetEntities()
		{
			return Set;
		}
	}
}