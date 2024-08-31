using System.Collections.Concurrent;
using BeautifulFundamental.Core.Extensions;
using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Server.Session.Context.Db;
using Microsoft.Extensions.Caching.Memory;

namespace BeautifulFundamental.Server.Db
{
	public interface IDbManager
	{
		IEnumerable<T>? GetEntities<T>(IIdentificationKey? sessionKey = null) where T : EntityDto;
		void SaveChanges<T>(T dto, IIdentificationKey? sessionKey = null) where T : EntityDto;
		IContextCollection? GetContextCollection(string requestedTypeName);
	}

	public class DbManager : IDbManager, IDisposable
	{
		private const string CacheKey = "DbManager_MasterCacheKey";

		private readonly ConcurrentDictionary<string, IDbContext> _dbContexts;

		private readonly IMemoryCache _cache;
		private readonly MemoryCacheEntryOptions _cacheOptions;

		public DbManager(IDbContextResolver dbContextResolver, IDbSettings dbSettings, IMemoryCache cache)
		{
			var dbContexts = dbContextResolver.Get();
			_dbContexts =
				new ConcurrentDictionary<string, IDbContext>(
					dbContexts.ToDictionary(context => context.Id));

			_cache = cache;

			_cacheOptions = new MemoryCacheEntryOptions()
				.SetSlidingExpiration(TimeSpan.FromSeconds(dbSettings.CachingTimeInSeconds));

			CacheDbContext();
		}

		private void CacheDbContext()
		{
			try
			{
				foreach (var dbContext in _dbContexts.Values)
				{
					var dbContextType = dbContext.GetType();

					if (dbContext.GetEntities() is not IEnumerable<EntityDto> dbContextEntities)
					{
						this.LogWarning($"[CacheDbContext] No entities found in {dbContextType.Name}");
						continue;
					}

					var contextEntities = dbContextEntities.ToList();
					if (!contextEntities.Any())
					{
						this.LogWarning($"[CacheDbContext] No entries found in {dbContextType.Name}");
						continue;
					}

					var cacheKey = CreateCacheKey(dbContext.TypeNameOfCollectionEntries);
					UpdateCache(cacheKey, contextEntities);
				}
			}
			catch (Exception ex)
			{
				this.LogError($"Error caching DbContexts: {ex.Message}");
			}
		}

		private void UpdateCache(string cacheKey, IEnumerable<EntityDto> dbContextEntities)
		{
			_cache.Set(cacheKey, dbContextEntities, _cacheOptions);
		}

		private static string CreateCacheKey(string name, string? sessionId = null)
		{
			var cleanTypeName = CleanTypeName(name);
			return $"{sessionId ?? CacheKey}_{cleanTypeName}";
		}

		public IEnumerable<T>? GetEntities<T>(IIdentificationKey? sessionKey = null) where T : EntityDto
		{
			var requestedType = typeof(T);
			return GetEntities<T>(sessionKey, requestedType.Name);
		}

		private IEnumerable<T>? GetEntities<T>(IIdentificationKey? sessionKey, string requestedTypeName)
			where T : EntityDto
		{
			var cacheKey = CreateCacheKey(requestedTypeName, sessionKey?.SessionId);

			if (!_cache.TryGetValue(cacheKey, out IEnumerable<EntityDto>? entities))
			{
				var foundContext = _dbContexts.Values.FirstOrDefault(dbContext =>
					dbContext.TypeNameOfCollectionEntries.Equals(requestedTypeName));

				entities = foundContext?.GetEntities().Cast<T>().ToList();

				if (entities == null)
				{
					this.LogWarning($"[GetEntities] No entities found for {requestedTypeName}");
					return Enumerable.Empty<T>();
				}

				UpdateCache(cacheKey, entities.ToList());
			}

			return entities?.Cast<T>();
		}

		public IContextCollection? GetContextCollection(string requestedTypeName)
		{
			return _dbContexts.Values.FirstOrDefault(dbContext =>
				dbContext.TypeNameOfCollectionEntries.Equals(requestedTypeName)) as IContextCollection;
		}

		public void SaveChanges<T>(T dto, IIdentificationKey? sessionKey = null) where T : EntityDto
		{
			var requestedTypeName = CleanTypeName(dto.TypeName);
			var cacheKey = CreateCacheKey(requestedTypeName, sessionKey?.SessionId);

			var foundDbContext = _dbContexts.Values.FirstOrDefault(dbContext =>
				dbContext.GetEntities() is IEnumerable<T> &&
				CleanTypeName(dbContext.TypeNameOfCollectionEntries) == requestedTypeName);

			if (foundDbContext == null)
			{
				this.LogError($"No DbContext found for {requestedTypeName}");
				return;
			}

			foundDbContext.AddEntity(dto);

			var entities = foundDbContext.GetEntities().Cast<EntityDto>().ToList();

			UpdateCache(cacheKey, entities);
		}


		/// <summary>
		/// Removes the Dto suffix from the typeName
		/// </summary>
		/// <param name="typeName">The name to prepare</param>
		/// <returns>Returns the prepared name</returns>
		private static string CleanTypeName(string typeName)
		{
			return typeName.Replace("Dto", "");
		}

		public void Dispose()
		{
			_cache.Dispose();
		}
	}
}