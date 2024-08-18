using Microsoft.Extensions.DependencyInjection;
using Session.Common.Contracts.Scope;
using Session.Common.Implementations;

namespace Session.Scope
{
	public class Scope : IScope
	{
		private readonly ISessionKey _sessionKey;

		private Scope(IServiceScope serviceScope, ISessionKey sessionKey)
		{
			_sessionKey = sessionKey;
			ServiceScope = serviceScope;
		}

		public IServiceScope ServiceScope { get; }
		public string Id => _sessionKey.SessionId;

		public T GetService<T>() where T : class
		{
			return ServiceScope.ServiceProvider.GetRequiredService<T>();
		}

		public static Scope Create(IServiceScope scope)
		{
			var sessionKey = scope.ServiceProvider.GetRequiredService<ISessionKey>();
			return new Scope(scope, sessionKey);
		}

		public void Dispose()
		{
			ServiceScope.Dispose();
		}
	}
}