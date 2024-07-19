using Core.Extensions;
using DbManagement.Common.Contracts;
using DbManagement.Common.Implementations;
using Microsoft.Extensions.Caching.Memory;
using Session.Common.Implementations;
using System.Collections.Concurrent;

namespace DbManagement
{
	public class DbManager : IDbManager
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
			foreach (var dbContext in _dbContexts.Values)
			{
				var dbContextType = dbContext.GetType();

				if (dbContext.GetEntities() is not IEnumerable<EntityDto> dbContextEntities)
				{
					this.LogError($"No entities found in {dbContextType.Name}");
					continue;
				}

				var cacheKey = CreateCacheKey(dbContext.TypeNameOfCollectionEntries);
				UpdateCache(cacheKey, dbContextEntities);
			}
		}

		private void UpdateCache(string cacheKey, IEnumerable<EntityDto> dbContextEntities)
		{
			_cache.Set(cacheKey, dbContextEntities.ToList(), _cacheOptions);
		}

		private static string CreateCacheKey(string name, string? sessionId = null)
		{
			var cleanTypeName = CleanTypeName(name);
			return $"{sessionId ?? CacheKey}_{cleanTypeName}";
		}

		public IEnumerable<T>? GetEntities<T>(ISessionKey? sessionKey = null) where T : EntityDto
		{
			var requestedType = typeof(T);
			var cacheKey = CreateCacheKey(requestedType.Name, sessionKey?.SessionId);

			if (!_cache.TryGetValue(cacheKey, out IEnumerable<EntityDto>? entities))
			{
				entities =
					_dbContexts.Values.FirstOrDefault(dbContext => dbContext.GetEntities() is IEnumerable<T>)
						?.GetEntities() as List<T>;

				if (entities == null)
				{
					this.LogError($"No entities found for {requestedType.Name}");
					return Enumerable.Empty<T>();
				}

				UpdateCache(cacheKey, entities);
			}

			return entities?.Cast<T>();
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
	}
}