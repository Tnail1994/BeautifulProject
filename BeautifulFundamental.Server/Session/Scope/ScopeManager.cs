using System.Collections.Concurrent;
using BeautifulFundamental.Core.Extensions;
using BeautifulFundamental.Core.Identification;
using BeautifulFundamental.Server.Session.Contracts.Scope;
using Microsoft.Extensions.DependencyInjection;

namespace BeautifulFundamental.Server.Session.Scope
{
	public class ScopeManager : IScopeManager, IDisposable
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly ConcurrentDictionary<string, IScope> _createdScopes = new();

		public ScopeManager(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public IScope Create()
		{
			var scope = Scope.Create(_serviceProvider.CreateScope());
			_createdScopes.TryAdd(scope.Id, scope);
			return scope;
		}

		public void Destroy(IIdentificationKey identificationKey)
		{
			if (_createdScopes.TryRemove(identificationKey.InstantiatedSessionId, out var scope))
			{
				scope.ServiceScope.Dispose();
			}
			else
			{
				this.LogError(
					$"Did not destroy session with instantiated id {identificationKey.InstantiatedSessionId} and current id {identificationKey.SessionId}");
			}
		}

		public void Dispose()
		{
			foreach (var scope in _createdScopes.Values)
			{
				scope.ServiceScope.Dispose();
			}

			_createdScopes.Clear();
		}
	}
}