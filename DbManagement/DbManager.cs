using System.Collections.Concurrent;
using DbManagement.Common.Contracts;
using System.Reflection;
using Core.Extensions;
using Microsoft.Extensions.Caching.Memory;
using SharedBeautifulData;
using Session.Common.Implementations;
using DbManagement.Common.Implementations;

namespace DbManagement
{
	public class DbManager : IDbManager
	{
		private const string CacheKey = "DbManager_MasterCacheKey";

		private static class EntityMapper
		{
			public static TTarget Map<TSource, TTarget>(TSource source)
				where TSource : class
				where TTarget : class, new()
			{
				if (source == null)
					throw new ArgumentNullException(nameof(source));

				var sourceProperties = typeof(TSource).GetProperties(BindingFlags.Public | BindingFlags.Instance);
				var targetProperties = typeof(TTarget).GetProperties(BindingFlags.Public | BindingFlags.Instance);

				var target = new TTarget();

				foreach (var sourceProperty in sourceProperties)
				{
					var targetProperty = targetProperties.FirstOrDefault(p =>
						p.Name == sourceProperty.Name && p.PropertyType == sourceProperty.PropertyType);

					if (targetProperty == null || !targetProperty.CanWrite)
						continue;

					var value = sourceProperty.GetValue(source);
					targetProperty.SetValue(target, value);
				}

				return target;
			}
		}

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

		private static string CreateCacheKey(string name)
		{
			return $"{CacheKey}_{name}".Replace("Dto", "");
		}

		public IEnumerable<T>? GetEntities<T>(ISessionKey? sessionKey = null) where T : Entity, new()
		{
			var requestedType = typeof(T);
			var cacheKey = sessionKey?.SessionId ?? CreateCacheKey(requestedType.Name);

			if (!_cache.TryGetValue(cacheKey, out IEnumerable<EntityDto>? entities))
			{
				entities =
					_dbContexts.Values.FirstOrDefault(dbContext => dbContext.GetEntities() is IEnumerable<T>)
						?.GetEntities() as List<EntityDto>;

				if (entities == null)
				{
					this.LogError($"No entities found for {requestedType.Name}");
					return Enumerable.Empty<T>();
				}

				UpdateCache(cacheKey, entities);
			}

			return entities?.Select(EntityMapper.Map<EntityDto, T>);
		}
	}
}