using Core.Extensions;
using DbManagement.Common.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Session.Common.Contracts;

namespace Session.Core
{
	public class SessionContextManager : ISessionContextManager
	{
		private readonly IDbManager _dbManager;
		private readonly IEnumerable<string> _collectionContextTypeNames;

		public SessionContextManager(IServiceProvider serviceProvider)
		{
			_dbManager = serviceProvider.GetRequiredService<IDbManager>();
			_collectionContextTypeNames = serviceProvider.GetServices<IContextCollection>()
				.Select(contextCollection => contextCollection.TypeNameOfCollectionEntries).ToList();
		}

		public bool TryFillSessionContext(ISessionContext sessionContext)
		{
			var neededTypes = _collectionContextTypeNames.Count();
			var addedTypes = 0;

			foreach (var collectionContextTypeName in _collectionContextTypeNames)
			{
				var contextCollection = _dbManager.GetContextCollection(collectionContextTypeName);
				var entry = contextCollection?.GetEntry(sessionContext.SessionId);

				if (entry == null)
				{
					this.LogError($"Did not get entry for {collectionContextTypeName}.\n" +
					              $"contextCollection is null: {contextCollection == null}");
					continue;
				}

				sessionContext.AddEntry(entry);
				addedTypes++;
			}

			return neededTypes == addedTypes;
		}
	}
}