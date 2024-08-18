using System.Collections.Concurrent;
using Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Session.Common.Contracts.Scope;
using Session.Common.Implementations;

namespace Session.Scope
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

		public void Destroy(ISessionKey sessionKey)
		{
			if (_createdScopes.TryRemove(sessionKey.InstantiatedSessionId, out var scope))
			{
				scope.ServiceScope.Dispose();
			}
			else
			{
				this.LogError(
					$"Did not destroy session with instantiated id {sessionKey.InstantiatedSessionId} and current id {sessionKey.SessionId}");
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