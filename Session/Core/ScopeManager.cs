using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Session.Common.Contracts;

namespace Session.Core
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

		public void Destroy(string id)
		{
			if (_createdScopes.TryRemove(id, out var scope))
			{
				scope.ServiceScope.Dispose();
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